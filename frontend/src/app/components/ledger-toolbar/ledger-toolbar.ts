import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideRotateCw } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';
import { LedgerSearch } from '../ledger-search/ledger-search';
import { StatusFilterBar } from '../status-filter/status-filter';

@Component({
  selector: 'app-ledger-toolbar',
  imports: [CommonModule, LedgerSearch, StatusFilterBar, LucideRotateCw],
  templateUrl: './ledger-toolbar.html',
})
export class LedgerToolbar {
  readonly ledgerService = inject(LedgerEntryService);
}
