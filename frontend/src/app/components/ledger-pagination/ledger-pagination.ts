import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  LucideChevronLeft,
  LucideChevronRight,
  LucideChevronsLeft,
  LucideChevronsRight,
} from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';

@Component({
  selector: 'app-ledger-pagination',
  imports: [
    CommonModule,
    LucideChevronLeft,
    LucideChevronRight,
    LucideChevronsLeft,
    LucideChevronsRight,
  ],
  templateUrl: './ledger-pagination.html',
})
export class LedgerPagination {
  readonly ledgerService = inject(LedgerEntryService);
  readonly pageSizes = [10, 25, 50, 100];

  goToPage(page: number): void {
    this.ledgerService.setPage(page);
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.ledgerService.setPageSize(Number(target.value));
  }
}
