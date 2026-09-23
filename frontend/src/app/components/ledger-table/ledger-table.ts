import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideChevronRight, LucideInbox } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';
import {
  LedgerEntryDetailDto,
  LedgerEntrySummaryDto,
  RegisteredAssetDto,
} from '../../models/ledger-entry.model';

interface DetailState {
  entry: LedgerEntryDetailDto | null;
  loading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-ledger-table',
  imports: [CommonModule, LucideChevronRight, LucideInbox],
  templateUrl: './ledger-table.html',
})
export class LedgerTable {
  readonly ledgerService = inject(LedgerEntryService);
  private readonly elRef = inject(ElementRef);
  readonly expandedRows = signal<Set<string>>(new Set());
  readonly expandedCodes = signal<Set<string>>(new Set());
  readonly detailStates = signal<Map<string, DetailState>>(new Map());

  isFocused(id: string): boolean {
    return this.ledgerService.selectedId() === id;
  }

  onRowCheck(item: LedgerEntrySummaryDto, event: Event): void {
    this.ledgerService.setRowChecked(item, (event.target as HTMLInputElement).checked);
  }

  onCodeCheck(entryId: string, codeId: string, event: Event): void {
    this.ledgerService.setCodeChecked(entryId, codeId, (event.target as HTMLInputElement).checked);
  }

  toggleCodeSelection(entryId: string, codeId: string): void {
    this.ledgerService.setCodeChecked(
      entryId,
      codeId,
      !this.ledgerService.isCodeChecked(entryId, codeId),
    );
  }

  isRowExpanded(id: string): boolean {
    return this.expandedRows().has(id);
  }

  toggleRow(item: LedgerEntrySummaryDto): void {
    this.ledgerService.selectEntry(item.id);
    const opening = !this.isRowExpanded(item.id);
    this.expandedRows.update((rows) => {
      const next = new Set(rows);
      if (opening) next.add(item.id);
      else next.delete(item.id);
      return next;
    });
  }

  loadDetail(id: string): void {
    if (this.detailStates().get(id)?.loading) return;
    this.detailStates.update((states) =>
      new Map(states).set(id, { entry: null, loading: true, error: null }),
    );
    this.ledgerService.getEntryDetail(id).subscribe({
      next: (entry) => {
        this.detailStates.update((states) =>
          new Map(states).set(id, { entry, loading: false, error: null }),
        );
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

  isCodeExpanded(entryId: string, codeId: string): boolean {
    return this.expandedCodes().has(`${entryId}:${codeId}`);
  }

  toggleCode(entryId: string, codeId: string): void {
    const key = `${entryId}:${codeId}`;
    const opening = !this.isCodeExpanded(entryId, codeId);
    this.expandedCodes.update((codes) => {
      const next = new Set(codes);
      if (opening) next.add(key);
      else next.delete(key);
      return next;
    });
    if (opening && !this.detailStates().has(entryId)) this.loadDetail(entryId);
  }

  assetsFor(entryId: string, codeId: string): RegisteredAssetDto[] {
    return (
      this.detailStates()
        .get(entryId)
        ?.entry?.codes.find((code) => code.id === codeId)?.assets ?? []
    );
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    const target = event.target as HTMLElement;
    if (
      target.tagName === 'INPUT' ||
      target.tagName === 'SELECT' ||
      target.tagName === 'TEXTAREA' ||
      target.tagName === 'BUTTON'
    ) {
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.ledgerService.selectNext();
      this.scrollSelectedIntoView();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.ledgerService.selectPrevious();
      this.scrollSelectedIntoView();
    }
  }

  private scrollSelectedIntoView(): void {
    setTimeout(() => {
      const selectedEl = this.elRef.nativeElement.querySelector('tr[data-focused="true"]');
      if (selectedEl) {
        selectedEl.scrollIntoView({ block: 'nearest' });
      }
    });
  }
}
