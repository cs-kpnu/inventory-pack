import { computed, inject, Injectable, signal } from '@angular/core';
import { LedgerEntryCodeDto, LedgerEntrySummaryDto } from '../models/ledger-entry.model';
import { LedgerExpansionService } from './ledger-expansion.service';

@Injectable({
  providedIn: 'root',
})
export class LedgerSelectionService {
  readonly selectedCodesByEntry = signal<Map<string, Set<string>>>(new Map());
  readonly selectedAssetIds = signal<Set<string>>(new Set());
  readonly selectedRowCount = computed(() => this.selectedCodesByEntry().size);
  readonly selectedCodeCount = computed(() =>
    Array.from(this.selectedCodesByEntry().values()).reduce(
      (count, codes) => count + codes.size,
      0,
    ),
  );
  readonly selectedAssetCount = computed(() => this.selectedAssetIds().size);
  readonly hasSelection = computed(
    () => this.selectedCodeCount() > 0 || this.selectedAssetCount() > 0,
  );
  private readonly expansion = inject(LedgerExpansionService);

  constructor() {
    // When details finish loading, sync any code that was checked beforehand
    this.expansion.onDetailLoaded((detail) => {
      const selectedCodes = this.selectedCodesByEntry().get(detail.id);
      if (!selectedCodes || selectedCodes.size === 0) return;

      this.selectedAssetIds.update((set) => {
        const next = new Set(set);
        for (const code of detail.codes) {
          if (selectedCodes.has(code.id)) {
            for (const asset of code.assets) {
              next.add(asset.id);
            }
          }
        }
        return next;
      });
    });
  }

  isAssetChecked(assetId: string): boolean {
    return this.selectedAssetIds().has(assetId);
  }

  isCodeIdChecked(entryId: string, codeId: string): boolean {
    return this.selectedCodesByEntry().get(entryId)?.has(codeId) ?? false;
  }

  isCodeChecked(entryId: string, code: LedgerEntryCodeDto): boolean {
    if (code.groupRegisteredAssetCount === 0) {
      return this.isCodeIdChecked(entryId, code.id);
    }
    const assets = this.expansion.assetsFor(entryId, code.id);
    if (assets.length > 0) {
      return assets.every((a) => this.isAssetChecked(a.id));
    }
    return this.isCodeIdChecked(entryId, code.id);
  }

  isCodeIndeterminate(entryId: string, code: LedgerEntryCodeDto): boolean {
    if (code.groupRegisteredAssetCount === 0) {
      return false;
    }
    const assets = this.expansion.assetsFor(entryId, code.id);
    if (assets.length === 0) {
      return false;
    }
    const selectedCount = assets.filter((a) => this.isAssetChecked(a.id)).length;
    return selectedCount > 0 && selectedCount < assets.length;
  }

  isRowChecked(entry: LedgerEntrySummaryDto): boolean {
    return (
      entry.codes.length > 0 && entry.codes.every((code) => this.isCodeChecked(entry.id, code))
    );
  }

  isRowPartiallyChecked(entry: LedgerEntrySummaryDto): boolean {
    if (this.isRowChecked(entry)) return false;
    return entry.codes.some(
      (code) => this.isCodeChecked(entry.id, code) || this.isCodeIndeterminate(entry.id, code),
    );
  }

  areAllVisibleRowsChecked(entries: LedgerEntrySummaryDto[]): boolean {
    return entries.length > 0 && entries.every((entry) => this.isRowChecked(entry));
  }

  areAnyVisibleRowsChecked(entries: LedgerEntrySummaryDto[]): boolean {
    return entries.some((entry) => this.isRowChecked(entry) || this.isRowPartiallyChecked(entry));
  }

  setAssetChecked(entryId: string, codeId: string, assetId: string, checked: boolean): void {
    this.selectedAssetIds.update((set) => {
      const next = new Set(set);
      if (checked) next.add(assetId);
      else next.delete(assetId);
      return next;
    });

    const assets = this.expansion.assetsFor(entryId, codeId);
    if (assets.length > 0) {
      const allChecked = assets.every((a) =>
        a.id === assetId ? checked : this.isAssetChecked(a.id),
      );
      this.setCodeIdChecked(entryId, codeId, allChecked);
    }
  }

  setCodeChecked(entry: LedgerEntrySummaryDto, code: LedgerEntryCodeDto, checked: boolean): void {
    this.setCodeIdChecked(entry.id, code.id, checked);

    if (code.groupRegisteredAssetCount > 0) {
      const assets = this.expansion.assetsFor(entry.id, code.id);
      if (assets.length > 0) {
        this.selectedAssetIds.update((set) => {
          const next = new Set(set);
          for (const asset of assets) {
            if (checked) next.add(asset.id);
            else next.delete(asset.id);
          }
          return next;
        });
      } else {
        this.expansion.loadDetail(entry.id, (detail) => {
          const loadedCode = detail.codes.find((c) => c.id === code.id);
          if (loadedCode) {
            this.selectedAssetIds.update((set) => {
              const next = new Set(set);
              for (const asset of loadedCode.assets) {
                if (checked) next.add(asset.id);
                else next.delete(asset.id);
              }
              return next;
            });
          }
        });
      }
    }
  }

  setRowChecked(entry: LedgerEntrySummaryDto, checked: boolean): void {
    for (const code of entry.codes) {
      this.setCodeChecked(entry, code, checked);
    }
  }

  setAllVisibleRowsChecked(entries: LedgerEntrySummaryDto[], checked: boolean): void {
    for (const entry of entries) {
      this.setRowChecked(entry, checked);
    }
  }

  clearSelection(): void {
    this.selectedCodesByEntry.set(new Map());
    this.selectedAssetIds.set(new Set());
  }

  setCodeIdChecked(entryId: string, codeId: string, checked: boolean): void {
    this.selectedCodesByEntry.update((selection) => {
      const next = new Map(selection);
      const codes = new Set(next.get(entryId) ?? []);
      if (checked) {
        codes.add(codeId);
      } else {
        codes.delete(codeId);
      }
      if (codes.size > 0) {
        next.set(entryId, codes);
      } else {
        next.delete(entryId);
      }
      return next;
    });
  }
}
