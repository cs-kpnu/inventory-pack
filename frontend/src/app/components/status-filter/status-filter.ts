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
  templateUrl: './status-filter.html',
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
