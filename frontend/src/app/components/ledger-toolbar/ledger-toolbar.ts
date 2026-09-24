import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucidePlus, LucideRotateCw } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';
import { LedgerSelectionService } from '../../services/ledger-selection.service';
import { LedgerExpansionService } from '../../services/ledger-expansion.service';
import { RegisterAssetsRequest, RegisteredAssetDto } from '../../models/ledger-entry.model';
import { LedgerSearch } from '../ledger-search/ledger-search';
import { StatusFilterBar } from '../status-filter/status-filter';
import { AssetRegistrationDialog } from '../asset-registration-dialog/asset-registration-dialog';

@Component({
  selector: 'app-ledger-toolbar',
  imports: [
    CommonModule,
    LedgerSearch,
    StatusFilterBar,
    AssetRegistrationDialog,
    LucideRotateCw,
    LucidePlus,
  ],
  templateUrl: './ledger-toolbar.html',
})
export class LedgerToolbar {
  readonly ledgerService = inject(LedgerEntryService);
  readonly selectionService = inject(LedgerSelectionService);
  readonly expansionService = inject(LedgerExpansionService);

  readonly registrationDialogOpen = signal(false);
  readonly registrationWasSuccessful = signal(false);

  readonly unlistedSelectedCodeCount = computed(() => {
    const selection = this.selectionService.selectedCodesByEntry();
    const representedCodeCount = this.selectionService.selectedEntries().reduce((count, entry) => {
      const selectedCodes = selection.get(entry.id);
      return count + entry.codes.filter((code) => selectedCodes?.has(code.id)).length;
    }, 0);

    return Math.max(0, this.selectionService.selectedCodeCount() - representedCodeCount);
  });

  openRegistrationDialog(): void {
    this.registrationWasSuccessful.set(false);
    this.registrationDialogOpen.set(true);
  }

  handleRegistrationSuccess(data: {
    requests: RegisterAssetsRequest[];
    results: RegisteredAssetDto[][];
  }): void {
    this.registrationWasSuccessful.set(true);

    for (const assetList of data.results) {
      this.selectionService.selectedAssetIds.update((set) => {
        const next = new Set(set);
        for (const asset of assetList) {
          next.add(asset.id);
        }
        return next;
      });
    }

    const entryIds = new Set<string>();
    for (const req of data.requests) {
      entryIds.add(req.ledgerEntryId);
      this.expansionService.expandRow(req.ledgerEntryId);
      this.expansionService.expandCode(req.ledgerEntryId, req.codeGroupId);
    }
    for (const entryId of entryIds) {
      this.expansionService.refreshDetail(entryId);
    }

    this.ledgerService.loadEntries(true);
  }

  closeRegistrationDialog(): void {
    this.registrationDialogOpen.set(false);
    this.registrationWasSuccessful.set(false);
  }
}
