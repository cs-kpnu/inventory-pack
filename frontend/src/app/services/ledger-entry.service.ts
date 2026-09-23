import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  LedgerEntryDetailDto,
  LedgerEntryListResponse,
  LedgerEntrySummaryDto,
  StatusFilter
} from '../models/ledger-entry.model';

@Injectable({
  providedIn: 'root',
})
export class LedgerEntryService {
  // State signals
  readonly entries = signal<LedgerEntrySummaryDto[]>([]);
  readonly totalCount = signal<number>(0);
  readonly page = signal<number>(1);
  readonly pageSize = signal<number>(25);
  readonly search = signal<string>('');
  readonly status = signal<StatusFilter>('all');
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);
  readonly hasRejectedRows = signal<boolean>(false);
  readonly selectedId = signal<string | null>(null);
  readonly selectedCodesByEntry = signal<Map<string, Set<string>>>(new Map());
  // Computed state
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
  readonly rangeStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1,
  );
  readonly rangeEnd = computed(() => Math.min(this.page() * this.pageSize(), this.totalCount()));
  readonly selectedEntry = computed(() => {
    const id = this.selectedId();
    if (!id) return null;
    return this.entries().find((e) => e.id === id) ?? null;
  });
  readonly selectedRowCount = computed(() => this.selectedCodesByEntry().size);
  readonly selectedCodeCount = computed(() =>
    Array.from(this.selectedCodesByEntry().values()).reduce((count, codes) => count + codes.size, 0),
  );
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/ledger-entries';

  getEntryDetail(id: string): Observable<LedgerEntryDetailDto> {
    return this.http.get<LedgerEntryDetailDto>(`${this.apiUrl}/${id}`);
  }

  loadEntries(): void {
    this.loading.set(true);
    this.error.set(null);

    let params = new HttpParams()
      .set('page', this.page().toString())
      .set('pageSize', this.pageSize().toString());

    const trimmedSearch = this.search().trim();
    if (trimmedSearch) {
      params = params.set('search', trimmedSearch);
    }

    const currentStatus = this.status();
    if (currentStatus && currentStatus !== 'all') {
      params = params.set('status', currentStatus);
    }

    this.http.get<LedgerEntryListResponse>(this.apiUrl, { params }).subscribe({
      next: (response) => {
        this.entries.set(response.items);
        this.totalCount.set(response.totalCount);
        this.page.set(response.page);
        this.pageSize.set(response.pageSize);
        this.hasRejectedRows.set(response.hasRejectedRows);
        this.loading.set(false);

        // Retain or select first if available
        const currentSelected = this.selectedId();
        const stillPresent = response.items.some((i) => i.id === currentSelected);
        if (!stillPresent && response.items.length > 0) {
          this.selectedId.set(response.items[0].id);
        } else if (response.items.length === 0) {
          this.selectedId.set(null);
        }
      },
      error: (err) => {
        const message =
          err?.error?.message || err?.message || 'Помилка під час завантаження об’єктів';
        this.error.set(message);
        this.loading.set(false);
      },
    });
  }

  selectEntry(id: string | null): void {
    this.selectedId.set(id);
  }

  isRowChecked(entry: LedgerEntrySummaryDto): boolean {
    const selectedCodes = this.selectedCodesByEntry().get(entry.id);
    return selectedCodes !== undefined && entry.codes.every((code) => selectedCodes.has(code.id));
  }

  isRowPartiallyChecked(entry: LedgerEntrySummaryDto): boolean {
    const selectedCodes = this.selectedCodesByEntry().get(entry.id);
    return selectedCodes !== undefined && selectedCodes.size > 0 && !this.isRowChecked(entry);
  }

  isCodeChecked(entryId: string, codeId: string): boolean {
    return this.selectedCodesByEntry().get(entryId)?.has(codeId) ?? false;
  }

  setRowChecked(entry: LedgerEntrySummaryDto, checked: boolean): void {
    this.setRowsChecked([entry], checked);
  }

  setRowsChecked(entries: readonly LedgerEntrySummaryDto[], checked: boolean): void {
    if (entries.length === 0) return;
    this.selectedCodesByEntry.update((selection) => {
      const next = new Map(selection);
      for (const entry of entries) {
        if (checked) {
          next.set(entry.id, new Set(entry.codes.map((code) => code.id)));
        } else {
          next.delete(entry.id);
        }
      }
      return next;
    });
  }

  setCodeChecked(entryId: string, codeId: string, checked: boolean): void {
    this.setCodesChecked([{ entryId, codeId }], checked);
  }

  setCodesChecked(
    selections: ReadonlyArray<{ entryId: string; codeId: string }>,
    checked: boolean,
  ): void {
    if (selections.length === 0) return;

    this.selectedCodesByEntry.update((selection) => {
      const next = new Map(selection);

      for (const { entryId, codeId } of selections) {
        const codes = new Set(next.get(entryId) ?? []);

        if (checked) {
          codes.add(codeId);
        } else {
          codes.delete(codeId);
        }

        if (codes.size > 0) {
          next.set(entryId, codes);
        } else {
          next.delete(entryId);
        }
      }

      return next;
    });
  }

  clearSelection(): void {
    this.selectedCodesByEntry.set(new Map());
  }

  selectNext(): void {
    const list = this.entries();
    if (list.length === 0) return;
    const current = this.selectedId();
    if (!current) {
      this.selectedId.set(list[0].id);
      return;
    }
    const idx = list.findIndex((e) => e.id === current);
    if (idx >= 0 && idx < list.length - 1) {
      this.selectedId.set(list[idx + 1].id);
    }
  }

  selectPrevious(): void {
    const list = this.entries();
    if (list.length === 0) return;
    const current = this.selectedId();
    if (!current) {
      this.selectedId.set(list[0].id);
      return;
    }
    const idx = list.findIndex((e) => e.id === current);
    if (idx > 0) {
      this.selectedId.set(list[idx - 1].id);
    }
  }

  setSearch(query: string): void {
    if (this.search() !== query) {
      this.search.set(query);
      this.page.set(1);
      this.loadEntries();
    }
  }

  setStatus(status: StatusFilter): void {
    if (this.status() !== status) {
      this.status.set(status);
      this.page.set(1);
      this.loadEntries();
    }
  }

  setPage(page: number): void {
    const validPage = Math.max(1, Math.min(page, this.totalPages()));
    if (this.page() !== validPage) {
      this.page.set(validPage);
      this.loadEntries();
    }
  }

  setPageSize(pageSize: number): void {
    if (this.pageSize() !== pageSize) {
      this.pageSize.set(pageSize);
      this.page.set(1);
      this.loadEntries();
    }
  }
}
