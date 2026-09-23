import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatusFilter } from '../../models/ledger-entry.model';
import { LedgerEntryService } from '../../services/ledger-entry.service';

interface StatusOption {
  value: StatusFilter;
  label: string;
}

@Component({
  selector: 'app-status-filter',
  imports: [CommonModule],
  template: `
    <div
      class="flex items-center gap-1 text-xs font-medium"
      role="group"
      aria-label="Фільтр за статусом"
    >
      @for (option of options; track option.value) {
        <button
          type="button"
          (click)="onSelect(option.value)"
          [class]="
            isActive(option.value)
              ? 'bg-zinc-900 text-white'
              : 'text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100'
          "
          class="px-3 py-1.5 rounded-md transition-colors cursor-pointer whitespace-nowrap"
        >
          {{ option.label }}
        </button>
      }
    </div>
  `,
})
export class StatusFilterBar {
  readonly options: StatusOption[] = [
    { value: 'all', label: 'Всі' },
    { value: 'unregistered', label: 'Не зареєстровані' },
    { value: 'partial', label: 'Частково' },
    { value: 'registered', label: 'Зареєстровані' },
  ];
  private readonly ledgerService = inject(LedgerEntryService);
  readonly currentStatus = this.ledgerService.status;

  isActive(status: StatusFilter): boolean {
    return this.currentStatus() === status;
  }

  onSelect(status: StatusFilter): void {
    this.ledgerService.setStatus(status);
  }
}
