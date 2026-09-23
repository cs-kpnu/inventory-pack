import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { LucideSearch, LucideX } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';

@Component({
  selector: 'app-ledger-search',
  imports: [ReactiveFormsModule, LucideSearch, LucideX],
  template: `
    <div class="relative flex items-center w-full">
      <div
        class="absolute inset-y-0 left-0 pl-2.5 flex items-center pointer-events-none text-zinc-500"
      >
        <svg lucideSearch [size]="15"></svg>
      </div>

      <input
        type="text"
        [formControl]="searchControl"
        (keydown.escape)="clear()"
        placeholder="Пошук (назва, код, МВО, субрахунок)..."
        class="w-full pl-8 pr-7 py-1.5 bg-zinc-100 hover:bg-zinc-200/60 focus:bg-white text-xs font-normal text-zinc-950 placeholder-zinc-500 rounded-md focus:outline-none focus:ring-2 focus:ring-zinc-900 transition-all"
      />

      @if (hasValue()) {
        <button
          type="button"
          (click)="clear()"
          class="absolute inset-y-0 right-0 pr-2 flex items-center text-zinc-500 hover:text-zinc-950 transition-colors cursor-pointer"
          title="Очистити"
        >
          <svg lucideX [size]="14"></svg>
        </button>
      }
    </div>
  `,
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
