import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LedgerEntryService } from './services/ledger-entry.service';
import { LedgerToolbar } from './components/ledger-toolbar/ledger-toolbar';

@Component({
  selector: 'app-root',
  imports: [CommonModule, LedgerToolbar],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  readonly ledgerService = inject(LedgerEntryService);

  ngOnInit(): void {
    this.ledgerService.loadEntries();
  }
}
