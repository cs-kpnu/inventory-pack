import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideChevronRight, LucideInbox } from '@lucide/angular';
import { LedgerEntryService } from '../../services/ledger-entry.service';
import {
  LedgerEntryDetailDto,
  LedgerEntrySummaryDto,
  RegisteredAssetDto,
} from '../../models/ledger-entry.model';

interface DetailState {
  entry: LedgerEntryDetailDto | null;
  loading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-ledger-table',
  imports: [CommonModule, LucideChevronRight, LucideInbox],
  template: `
    <div
      class="flex-1 min-h-0 overflow-auto bg-white focus:outline-none"
      style="scrollbar-gutter: stable"
      tabindex="0"
    >
      <table class="w-full table-fixed text-left border-collapse text-xs">
        <thead class="sticky top-0 bg-zinc-50 border-b border-zinc-200 z-10">
          <tr class="text-[11px] font-semibold text-zinc-600 uppercase tracking-wider">
            <th class="w-9 pl-3 pr-1"><span class="sr-only">Вибір</span></th>
            <th class="py-2.5 pl-4 pr-2 w-20 text-right">#</th>
            <th class="py-2.5 px-3 w-24">Рахунок</th>
            <th class="py-2.5 px-3">Найменування</th>
            <th class="py-2.5 px-3 w-48">МВО</th>
            <th class="py-2.5 px-3 w-20 text-right">К-сть</th>
            <th class="py-2.5 px-2 w-16">Од.</th>
            <th class="py-2.5 pl-3 pr-4 w-52">Коди</th>
          </tr>
        </thead>

        <tbody class="divide-y divide-zinc-100">
          @for (item of ledgerService.entries(); track item.id) {
            <tr
              (click)="toggleRow(item)"
              [attr.data-focused]="isFocused(item.id)"
              [class]="
                isRowExpanded(item.id)
                  ? 'bg-zinc-200/80 text-zinc-950'
                  : isFocused(item.id)
                    ? 'bg-zinc-100 text-zinc-950'
                    : 'hover:bg-zinc-50/80 text-zinc-800'
              "
              class="cursor-pointer transition-colors"
            >
              <td class="relative w-9 p-0" (click)="$event.stopPropagation()">
                <label
                  class="absolute inset-x-0 -top-px -bottom-px flex cursor-pointer items-center justify-center pl-1"
                >
                  <input
                    type="checkbox"
                    [checked]="ledgerService.isRowChecked(item)"
                    [indeterminate]="ledgerService.isRowPartiallyChecked(item)"
                    [attr.aria-label]="'Вибрати рядок ' + item.sourceRowNumber + ' і всі його коди'"
                    (change)="onRowCheck(item, $event)"
                    class="h-3.5 w-3.5 cursor-pointer accent-zinc-900"
                  />
                </label>
              </td>
              <!-- Row number -->
              <td class="py-2 pl-4 pr-2 font-mono text-zinc-400 text-[11px]">
                <div class="flex items-center justify-between gap-1">
                  <button
                    type="button"
                    (click)="toggleRow(item); $event.stopPropagation()"
                    [attr.aria-expanded]="isRowExpanded(item.id)"
                    [attr.aria-label]="
                      (isRowExpanded(item.id) ? 'Згорнути' : 'Розгорнути') +
                      ' коди рядка ' +
                      item.sourceRowNumber
                    "
                    class="rounded p-0.5 focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-zinc-700"
                  >
                    <svg
                      lucideChevronRight
                      [size]="13"
                      [class.rotate-90]="isRowExpanded(item.id)"
                    ></svg>
                  </button>
                  <span>{{ item.sourceRowNumber }}</span>
                </div>
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

              <!-- MVO -->
              <td class="py-2 px-3">
                @if (item.mvo) {
                  <span class="text-zinc-800">{{ item.mvo }}</span>
                } @else {
                  <span class="text-zinc-400 italic">Не вказано</span>
                }
              </td>

              <!-- Quantity -->
              <td class="py-2 px-3 text-right font-mono text-zinc-950">
                {{ item.quantity | number: '1.0-4' }}
              </td>

              <!-- Unit -->
              <td class="py-2 px-2 text-zinc-500">
                {{ item.unit || '—' }}
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
            @if (isRowExpanded(item.id)) {
              @for (code of item.codes; track code.id; let odd = $odd) {
                <tr
                  (click)="toggleCodeSelection(item.id, code.id)"
                  [class]="odd ? 'bg-zinc-100' : 'bg-white'"
                  class="cursor-pointer border-b border-zinc-200 text-zinc-900"
                >
                  <td></td>
                  <td class="relative p-0">
                    <label
                      class="absolute inset-x-0 -top-px -bottom-px flex cursor-pointer items-center justify-center"
                      (click)="$event.stopPropagation()"
                    >
                      <input
                        type="checkbox"
                        [checked]="ledgerService.isCodeChecked(item.id, code.id)"
                        [attr.aria-label]="
                          'Вибрати код ' + code.code + ' у рядку ' + item.sourceRowNumber
                        "
                        (change)="onCodeCheck(item.id, code.id, $event)"
                        class="h-3.5 w-3.5 cursor-pointer accent-zinc-900"
                      />
                    </label>
                  </td>
                  <td colspan="2" class="py-2 pl-2 pr-2">
                    <div class="flex items-center gap-2">
                      @if (code.groupRegisteredAssetCount > 0) {
                        <button
                          type="button"
                          (click)="toggleCode(item.id, code.id); $event.stopPropagation()"
                          [attr.aria-expanded]="isCodeExpanded(item.id, code.id)"
                          [attr.aria-label]="
                            (isCodeExpanded(item.id, code.id) ? 'Згорнути' : 'Показати') +
                            ' об’єкти коду ' +
                            code.code
                          "
                          class="flex items-center gap-2 rounded py-0.5 pr-2 focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-zinc-700"
                        >
                          <svg
                            lucideChevronRight
                            [size]="13"
                            class="shrink-0 text-zinc-500"
                            [class.rotate-90]="isCodeExpanded(item.id, code.id)"
                          ></svg>
                          <span class="font-mono font-semibold">{{ code.code }}</span>
                        </button>
                      } @else {
                        <span class="ml-[21px] font-mono font-semibold text-zinc-900">{{
                          code.code
                        }}</span>
                      }
                      @if (code.sourceRowCount > 1) {
                        <span class="text-[11px] text-zinc-500"
                          >У {{ code.sourceRowCount }} рядках</span
                        >
                      }
                    </div>
                  </td>
                  <td></td>
                  <td></td>
                  <td></td>
                  <td class="pl-3 pr-4 text-xs text-zinc-800">
                    @if (code.groupRegisteredAssetCount === 0) {
                      Об'єктів немає
                    } @else {
                      У групі: {{ code.groupRegisteredAssetCount }}
                    }
                  </td>
                </tr>
                @if (isCodeExpanded(item.id, code.id)) {
                  <tr class="bg-zinc-50 text-xs text-zinc-800">
                    <td></td>
                    <td></td>
                    <td colspan="6" class="py-1.5 pl-5 pr-4">
                      @if (detailStates().get(item.id); as state) {
                        @if (state.loading) {
                          <span class="text-zinc-500">Завантаження об'єктів…</span>
                        } @else if (state.error) {
                          <span class="text-red-700">{{ state.error }}</span>
                          <button
                            type="button"
                            class="ml-2 underline"
                            (click)="loadDetail(item.id)"
                          >
                            Повторити
                          </button>
                        } @else {
                          <div class="divide-y divide-zinc-200">
                            @for (asset of assetsFor(item.id, code.id); track asset.id) {
                              <div class="flex flex-wrap items-center gap-x-6 gap-y-1 py-1.5">
                                <span class="font-mono text-zinc-900" [title]="asset.payload">{{
                                  asset.payload
                                }}</span>
                                <span>МВО: {{ asset.registeredForMvo || 'Не вказано' }}</span>
                                <span class="text-zinc-500"
                                  >Зареєстровано:
                                  {{ asset.allocatedAt | date: 'dd.MM.yyyy HH:mm' }}</span
                                >
                              </div>
                            } @empty {
                              <span class="text-zinc-500">Об'єктів немає</span>
                            }
                          </div>
                        }
                      }
                    </td>
                  </tr>
                }
              } @empty {
                <tr class="bg-zinc-50">
                  <td colspan="8" class="py-3 pl-12 text-xs text-zinc-500">Кодів не знайдено</td>
                </tr>
              }
            }
          } @empty {
            @if (!ledgerService.loading()) {
              <tr>
                <td colspan="8" class="py-16 text-center text-zinc-400">
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
  readonly expandedRows = signal<Set<string>>(new Set());
  readonly expandedCodes = signal<Set<string>>(new Set());
  readonly detailStates = signal<Map<string, DetailState>>(new Map());

  isFocused(id: string): boolean {
    return this.ledgerService.selectedId() === id;
  }

  onRowCheck(item: LedgerEntrySummaryDto, event: Event): void {
    this.ledgerService.setRowChecked(item, (event.target as HTMLInputElement).checked);
  }

  onCodeCheck(entryId: string, codeId: string, event: Event): void {
    this.ledgerService.setCodeChecked(entryId, codeId, (event.target as HTMLInputElement).checked);
  }

  toggleCodeSelection(entryId: string, codeId: string): void {
    this.ledgerService.setCodeChecked(
      entryId,
      codeId,
      !this.ledgerService.isCodeChecked(entryId, codeId),
    );
  }

  isRowExpanded(id: string): boolean {
    return this.expandedRows().has(id);
  }

  toggleRow(item: LedgerEntrySummaryDto): void {
    this.ledgerService.selectEntry(item.id);
    const opening = !this.isRowExpanded(item.id);
    this.expandedRows.update((rows) => {
      const next = new Set(rows);
      if (opening) next.add(item.id);
      else next.delete(item.id);
      return next;
    });
  }

  loadDetail(id: string): void {
    if (this.detailStates().get(id)?.loading) return;
    this.detailStates.update((states) =>
      new Map(states).set(id, { entry: null, loading: true, error: null }),
    );
    this.ledgerService.getEntryDetail(id).subscribe({
      next: (entry) => {
        this.detailStates.update((states) =>
          new Map(states).set(id, { entry, loading: false, error: null }),
        );
      },
      error: () => {
        this.detailStates.update((states) =>
          new Map(states).set(id, {
            entry: null,
            loading: false,
            error: 'Не вдалося завантажити коди та об’єкти',
          }),
        );
      },
    });
  }

  isCodeExpanded(entryId: string, codeId: string): boolean {
    return this.expandedCodes().has(`${entryId}:${codeId}`);
  }

  toggleCode(entryId: string, codeId: string): void {
    const key = `${entryId}:${codeId}`;
    const opening = !this.isCodeExpanded(entryId, codeId);
    this.expandedCodes.update((codes) => {
      const next = new Set(codes);
      if (opening) next.add(key);
      else next.delete(key);
      return next;
    });
    if (opening && !this.detailStates().has(entryId)) this.loadDetail(entryId);
  }

  assetsFor(entryId: string, codeId: string): RegisteredAssetDto[] {
    return (
      this.detailStates()
        .get(entryId)
        ?.entry?.codes.find((code) => code.id === codeId)?.assets ?? []
    );
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    const target = event.target as HTMLElement;
    if (
      target.tagName === 'INPUT' ||
      target.tagName === 'SELECT' ||
      target.tagName === 'TEXTAREA' ||
      target.tagName === 'BUTTON'
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
      const selectedEl = this.elRef.nativeElement.querySelector('tr[data-focused="true"]');
      if (selectedEl) {
        selectedEl.scrollIntoView({ block: 'nearest' });
      }
    });
  }
}
