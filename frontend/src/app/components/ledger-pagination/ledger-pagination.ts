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
  template: `
    <div
      class="flex flex-wrap items-center justify-between gap-3 px-4 py-2 bg-white border-t border-zinc-200 text-xs text-zinc-600 shrink-0"
    >
      <!-- Range indicator -->
      <div>
        Показано
        <strong class="font-bold text-zinc-950"
          >{{ ledgerService.rangeStart() }}–{{ ledgerService.rangeEnd() }}</strong
        >
        з
        <strong class="font-bold text-zinc-950">{{ ledgerService.totalCount() | number }}</strong>
      </div>

      <!-- Controls group -->
      <div class="flex items-center gap-6">
        <!-- Page size selector -->
        <div class="flex items-center gap-1.5">
          <span class="text-zinc-500">На сторінці:</span>
          <select
            [value]="ledgerService.pageSize()"
            (change)="onPageSizeChange($event)"
            class="bg-zinc-100 text-zinc-950 font-medium rounded px-2 py-1 focus:outline-none focus:ring-1 focus:ring-zinc-900 cursor-pointer text-xs"
          >
            @for (size of pageSizes; track size) {
              <option [value]="size">{{ size }}</option>
            }
          </select>
        </div>

        <!-- Page navigation -->
        <div class="flex items-center gap-1">
          <button
            type="button"
            (click)="goToPage(1)"
            [disabled]="ledgerService.page() <= 1"
            class="p-1 rounded text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
            title="Перша сторінка"
          >
            <svg lucideChevronsLeft [size]="15"></svg>
          </button>

          <button
            type="button"
            (click)="goToPage(ledgerService.page() - 1)"
            [disabled]="ledgerService.page() <= 1"
            class="p-1 rounded text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
            title="Попередня сторінка"
          >
            <svg lucideChevronLeft [size]="15"></svg>
          </button>

          <span class="px-2 font-medium text-zinc-950">
            {{ ledgerService.page() }}
            <span class="text-zinc-400 font-normal">/</span>
            {{ ledgerService.totalPages() }}
          </span>

          <button
            type="button"
            (click)="goToPage(ledgerService.page() + 1)"
            [disabled]="ledgerService.page() >= ledgerService.totalPages()"
            class="p-1 rounded text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
            title="Наступна сторінка"
          >
            <svg lucideChevronRight [size]="15"></svg>
          </button>

          <button
            type="button"
            (click)="goToPage(ledgerService.totalPages())"
            [disabled]="ledgerService.page() >= ledgerService.totalPages()"
            class="p-1 rounded text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors cursor-pointer"
            title="Остання сторінка"
          >
            <svg lucideChevronsRight [size]="15"></svg>
          </button>
        </div>
      </div>
    </div>
  `,
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
