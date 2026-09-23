import { Component, ElementRef, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideInbox } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';

@Component({
  selector: 'app-ledger-table',
  imports: [CommonModule, LucideInbox],
  template: `
    <div class="flex-1 min-h-0 overflow-auto bg-white select-none focus:outline-none" tabindex="0">
      <table class="w-full text-left border-collapse text-xs">
        <thead class="sticky top-0 bg-zinc-50 border-b border-zinc-200 z-10">
          <tr class="text-[11px] font-semibold text-zinc-600 uppercase tracking-wider">
            <th class="py-2.5 pl-4 pr-2 w-12 text-right">#</th>
            <th class="py-2.5 px-3 w-24">Рахунок</th>
            <th class="py-2.5 px-3">Найменування</th>
            <th class="py-2.5 px-3 w-20 text-right">К-сть</th>
            <th class="py-2.5 px-2 w-16">Од.</th>
            <th class="py-2.5 px-3 w-48">МВО</th>
            <th class="py-2.5 pl-3 pr-4 w-52">Коди</th>
          </tr>
        </thead>

        <tbody class="divide-y divide-zinc-100">
          @for (item of ledgerService.entries(); track item.id) {
            <tr
              (click)="onSelect(item.id)"
              [class]="
                isSelected(item.id)
                  ? 'bg-zinc-100 text-zinc-950 font-medium'
                  : 'hover:bg-zinc-50/80 text-zinc-800'
              "
              class="cursor-pointer transition-colors"
            >
              <!-- Row number -->
              <td class="py-2 pl-4 pr-2 text-right font-mono text-zinc-400 text-[11px]">
                {{ item.sourceRowNumber }}
              </td>

              <!-- Subaccount -->
              <td class="py-2 px-3 font-mono text-zinc-600">
                {{ item.subaccount }}
              </td>

              <!-- Name -->
              <td class="py-2 px-3 text-zinc-950">
                <div class="line-clamp-1" [title]="item.name">
                  {{ item.name }}
                </div>
              </td>

              <!-- Quantity -->
              <td class="py-2 px-3 text-right font-mono text-zinc-950">
                {{ item.quantity | number: '1.0-4' }}
              </td>

              <!-- Unit -->
              <td class="py-2 px-2 text-zinc-500">
                {{ item.unit || '—' }}
              </td>

              <!-- MVO -->
              <td class="py-2 px-3">
                @if (item.mvo) {
                  <span class="text-zinc-800">{{ item.mvo }}</span>
                } @else {
                  <span class="text-zinc-400 italic">Не вказано</span>
                }
              </td>

              <!-- Codes -->
              <td class="py-2 pl-3 pr-4">
                <div class="flex flex-wrap items-center gap-1">
                  @for (code of item.codes.slice(0, 2); track code.id) {
                    <span
                      [class]="
                        code.groupRegisteredAssetCount > 0
                          ? 'bg-zinc-200 text-zinc-950 font-medium'
                          : 'bg-zinc-100 text-zinc-600'
                      "
                      class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-[11px] font-mono"
                    >
                      <span>{{ code.code }}</span>
                      @if (code.groupRegisteredAssetCount > 0) {
                        <span
                          class="w-1.5 h-1.5 rounded-full bg-emerald-600"
                          title="Зареєстровано об'єктів: {{ code.groupRegisteredAssetCount }}"
                        ></span>
                      }
                    </span>
                  }
                  @if (item.codes.length > 2) {
                    <span
                      class="px-1 text-[10px] text-zinc-400 font-mono"
                      title="Ще {{ item.codes.length - 2 }} кодів"
                    >
                      +{{ item.codes.length - 2 }}
                    </span>
                  }
                </div>
              </td>
            </tr>
          } @empty {
            @if (!ledgerService.loading()) {
              <tr>
                <td colspan="7" class="py-16 text-center text-zinc-400">
                  <div class="inline-flex p-3 rounded-full bg-zinc-100 text-zinc-400 mb-2">
                    <svg lucideInbox [size]="20"></svg>
                  </div>
                  <div class="text-xs font-medium text-zinc-600">Записів не знайдено</div>
                  <div class="text-[11px] text-zinc-400 mt-0.5">
                    Спробуйте змінити фільтр або пошуковий запит
                  </div>
                </td>
              </tr>
            }
          }
        </tbody>
      </table>
    </div>
  `,
})
export class LedgerTable {
  readonly ledgerService = inject(LedgerEntryService);
  private readonly elRef = inject(ElementRef);

  isSelected(id: string): boolean {
    return this.ledgerService.selectedId() === id;
  }

  onSelect(id: string): void {
    this.ledgerService.selectEntry(id);
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    const target = event.target as HTMLElement;
    if (
      target.tagName === 'INPUT' ||
      target.tagName === 'SELECT' ||
      target.tagName === 'TEXTAREA'
    ) {
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.ledgerService.selectNext();
      this.scrollSelectedIntoView();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.ledgerService.selectPrevious();
      this.scrollSelectedIntoView();
    }
  }

  private scrollSelectedIntoView(): void {
    setTimeout(() => {
      const selectedEl = this.elRef.nativeElement.querySelector('.bg-zinc-100');
      if (selectedEl) {
        selectedEl.scrollIntoView({ block: 'nearest' });
      }
    });
  }
}
