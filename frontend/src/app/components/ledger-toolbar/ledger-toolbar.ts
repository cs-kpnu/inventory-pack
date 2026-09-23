import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideRotateCw } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';
import { LedgerSearch } from '../ledger-search/ledger-search';
import { StatusFilterBar } from '../status-filter/status-filter';

@Component({
  selector: 'app-ledger-toolbar',
  imports: [CommonModule, LedgerSearch, StatusFilterBar, LucideRotateCw],
  template: `
    <div
      class="flex flex-wrap items-center justify-between gap-4 px-4 py-2 bg-white border-b border-zinc-200"
    >
      <div class="flex flex-wrap items-center gap-4 flex-1 min-w-[300px]">
        <div class="w-80">
          <app-ledger-search />
        </div>
        <app-status-filter />
      </div>

      <div class="flex items-center gap-3 text-xs text-zinc-600">
        @if (ledgerService.selectedCodeCount() > 0) {
          <span class="text-zinc-600">
            Вибрано кодів:
            <span class="font-semibold text-zinc-950">{{ ledgerService.selectedCodeCount() }}</span>
          </span>
          <button
            type="button"
            (click)="ledgerService.clearSelection()"
            class="cursor-pointer rounded border border-zinc-300 bg-white px-2 py-1 font-medium text-zinc-700 hover:bg-zinc-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-700"
          >
            Скинути
          </button>
        }
        <span>
          Всього:
          <span class="font-bold text-zinc-950">{{ ledgerService.totalCount() | number }}</span>
        </span>

        <button
          type="button"
          (click)="ledgerService.loadEntries()"
          [disabled]="ledgerService.loading()"
          class="p-1.5 text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 rounded-md transition-colors cursor-pointer disabled:opacity-40"
          title="Оновити"
        >
          <svg lucideRotateCw [size]="14" [class.animate-spin]="ledgerService.loading()"></svg>
        </button>
      </div>
    </div>
  `,
})
export class LedgerToolbar {
  readonly ledgerService = inject(LedgerEntryService);
}
