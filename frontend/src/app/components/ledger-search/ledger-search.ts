import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { LucideSearch, LucideX } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';

@Component({
  selector: 'app-ledger-search',
  imports: [ReactiveFormsModule, LucideSearch, LucideX],
  templateUrl: './ledger-search.html',
})
export class LedgerSearch implements OnInit {
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly hasValue = signal<boolean>(false);
  private readonly ledgerService = inject(LedgerEntryService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const initialQuery = this.ledgerService.search();
    if (initialQuery) {
      this.searchControl.setValue(initialQuery, { emitEvent: false });
      this.hasValue.set(true);
    }

    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((query) => {
        this.hasValue.set(query.length > 0);
        this.ledgerService.setSearch(query);
      });
  }

  clear(): void {
    this.searchControl.setValue('');
  }
}
