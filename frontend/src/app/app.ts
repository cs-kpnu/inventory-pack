import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LedgerEntryService } from './services/ledger-entry.service';
import { LedgerToolbar } from './components/ledger-toolbar/ledger-toolbar';
import { LedgerTable } from './components/ledger-table/ledger-table';
import { LedgerPagination } from './components/ledger-pagination/ledger-pagination';

@Component({
  selector: 'app-root',
  imports: [CommonModule, LedgerToolbar, LedgerTable, LedgerPagination],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  readonly ledgerService = inject(LedgerEntryService);
}
