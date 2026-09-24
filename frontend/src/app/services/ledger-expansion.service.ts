import { inject, Injectable, signal } from '@angular/core';
import {
  DetailState,
  LedgerEntryDetailDto,
  LedgerEntrySummaryDto,
  RegisteredAssetDto,
} from '../models/ledger-entry.model';
import { LedgerApiService } from './ledger-api.service';

@Injectable({
  providedIn: 'root',
})
export class LedgerExpansionService {
  readonly expandedRows = signal<Set<string>>(new Set());
  readonly expandedCodes = signal<Set<string>>(new Set());
  readonly detailStates = signal<Map<string, DetailState>>(new Map());
  private readonly api = inject(LedgerApiService);
  private readonly detailLoadedHooks = new Set<(detail: LedgerEntryDetailDto) => void>();

  onDetailLoaded(hook: (detail: LedgerEntryDetailDto) => void): () => void {
    this.detailLoadedHooks.add(hook);
    return () => this.detailLoadedHooks.delete(hook);
  }

  clearExpansion(): void {
    this.expandedRows.set(new Set());
    this.expandedCodes.set(new Set());
  }

  expandRow(id: string): void {
    this.expandedRows.update((rows) => new Set(rows).add(id));
  }

  expandCode(entryId: string, codeId: string): void {
    this.expandedCodes.update((codes) => new Set(codes).add(`${entryId}:${codeId}`));
  }

  isRowExpanded(id: string): boolean {
    return this.expandedRows().has(id);
  }

  areAllVisibleRowsExpanded(entries: LedgerEntrySummaryDto[]): boolean {
    return entries.length > 0 && entries.every((entry) => this.expandedRows().has(entry.id));
  }

  toggleVisibleRows(entries: LedgerEntrySummaryDto[]): void {
    if (entries.length === 0) return;
    const collapse = entries.every((entry) => this.expandedRows().has(entry.id));

    this.expandedRows.update((rows) => {
      const next = new Set(rows);
      for (const entry of entries) {
        if (collapse) next.delete(entry.id);
        else next.add(entry.id);
      }
      return next;
    });

    this.expandedCodes.update((codes) => {
      const next = new Set(codes);
      for (const entry of entries) {
        for (const code of entry.codes) {
          const codeKey = `${entry.id}:${code.id}`;
          if (!collapse && code.groupRegisteredAssetCount > 0) {
            next.add(codeKey);
          } else {
            next.delete(codeKey);
          }
        }
      }
      return next;
    });

    if (!collapse) {
      for (const entry of entries) {
        if (entry.codes.some((c) => c.groupRegisteredAssetCount > 0)) {
          if (!this.detailStates().has(entry.id)) {
            this.loadDetail(entry.id);
          }
        }
      }
    }
  }

  toggleRow(entry: LedgerEntrySummaryDto): void {
    const opening = !this.isRowExpanded(entry.id);
    this.expandedRows.update((rows) => {
      const next = new Set(rows);
      if (opening) next.add(entry.id);
      else next.delete(entry.id);
      return next;
    });

    this.expandedCodes.update((codes) => {
      const next = new Set(codes);
      for (const code of entry.codes) {
        const codeKey = `${entry.id}:${code.id}`;
        if (opening && code.groupRegisteredAssetCount > 0) {
          next.add(codeKey);
        } else {
          next.delete(codeKey);
        }
      }
      return next;
    });

    if (opening && entry.codes.some((c) => c.groupRegisteredAssetCount > 0)) {
      if (!this.detailStates().has(entry.id)) {
        this.loadDetail(entry.id);
      }
    }
  }

  isCodeExpanded(entryId: string, codeId: string): boolean {
    return this.expandedCodes().has(`${entryId}:${codeId}`);
  }

  toggleCode(entryId: string, codeId: string, groupRegisteredAssetCount: number): void {
    if (groupRegisteredAssetCount <= 0) return;

    const key = `${entryId}:${codeId}`;
    const opening = !this.isCodeExpanded(entryId, codeId);
    this.expandedCodes.update((codes) => {
      const next = new Set(codes);
      if (opening) next.add(key);
      else next.delete(key);
      return next;
    });
    if (opening && !this.detailStates().has(entryId)) {
      this.loadDetail(entryId);
    }
  }

  assetsFor(entryId: string, codeId: string): RegisteredAssetDto[] {
    return (
      this.detailStates()
        .get(entryId)
        ?.entry?.codes.find((code) => code.id === codeId)?.assets ?? []
    );
  }

  loadDetail(id: string, onLoaded?: (detail: LedgerEntryDetailDto) => void): void {
    const current = this.detailStates().get(id);
    if (current?.loading) return;
    if (current?.entry) {
      onLoaded?.(current.entry);
      return;
    }
    this.detailStates.update((states) =>
      new Map(states).set(id, { entry: null, loading: true, error: null }),
    );
    this.api.getEntryDetail(id).subscribe({
      next: (entry) => {
        this.detailStates.update((states) =>
          new Map(states).set(id, { entry, loading: false, error: null }),
        );
        for (const hook of this.detailLoadedHooks) {
          hook(entry);
        }
        onLoaded?.(entry);
      },
      error: () => {
        this.detailStates.update((states) =>
          new Map(states).set(id, {
            entry: null,
            loading: false,
            error: 'Не вдалося завантажити коди та об’єкти',
          }),
        );
      },
    });
  }

  refreshDetail(id: string, onLoaded?: (detail: LedgerEntryDetailDto) => void): void {
    this.detailStates.update((states) => {
      const next = new Map(states);
      next.delete(id);
      return next;
    });
    this.loadDetail(id, onLoaded);
  }
}
