import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { LedgerEntrySummaryDto, StatusFilter } from '../models/ledger-entry.model';
import { LedgerApiService } from './ledger-api.service';
import { LedgerExpansionService } from './ledger-expansion.service';
import { LedgerSelectionService } from './ledger-selection.service';

@Injectable({
  providedIn: 'root',
})
export class LedgerEntryService {
  readonly selection = inject(LedgerSelectionService);
  readonly expansion = inject(LedgerExpansionService);
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
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly api = inject(LedgerApiService);
  private initialized = false;

  constructor() {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.handleQueryParams(params);
    });
  }

  // Backward compatibility getters for selection
  get selectedAssetIds() {
    return this.selection.selectedAssetIds;
  }
  get selectedCodesByEntry() {
    return this.selection.selectedCodesByEntry;
  }
  get selectedRowCount() {
    return this.selection.selectedRowCount;
  }
  get selectedCodeCount() {
    return this.selection.selectedCodeCount;
  }
  get selectedAssetCount() {
    return this.selection.selectedAssetCount;
  }
  get hasSelection() {
    return this.selection.hasSelection;
  }
  get selectedEntries() {
    return this.selection.selectedEntries;
  }

  loadEntries(preserveExpansion = false): void {
    this.loading.set(true);
    this.error.set(null);
    if (!preserveExpansion) {
      this.expansion.clearExpansion();
    }

    this.api
      .getEntries({
        page: this.page(),
        pageSize: this.pageSize(),
        search: this.search(),
        status: this.status(),
      })
      .subscribe({
        next: (response) => {
          if (!preserveExpansion) {
            this.expansion.clearExpansion();
          }
          this.entries.set(response.items);
          this.selection.syncEntrySnapshots(response.items);
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
    const trimmed = query.trim();
    if (this.search() !== trimmed) {
      this.updateUrl({ search: trimmed, page: 1 });
    }
  }

  setStatus(status: StatusFilter): void {
    if (this.status() !== status) {
      this.updateUrl({ status, page: 1 });
    }
  }

  setPage(page: number): void {
    const validPage = Math.max(1, Math.min(page, this.totalPages()));
    if (this.page() !== validPage) {
      this.updateUrl({ page: validPage });
    }
  }

  setPageSize(pageSize: number): void {
    if (this.pageSize() !== pageSize) {
      this.updateUrl({ pageSize, page: 1 });
    }
  }

  clearSelection(): void {
    this.selection.clearSelection();
  }

  resetAfterImport(): void {
    this.selection.clearSelection();

    if (this.search() !== '' || this.status() !== 'all' || this.page() !== 1) {
      this.updateUrl({ search: '', status: 'all', page: 1 });
      return;
    }

    this.loadEntries();
  }

  getEntryDetail(id: string) {
    return this.api.getEntryDetail(id);
  }

  private updateUrl(changes: {
    search?: string;
    status?: StatusFilter;
    page?: number;
    pageSize?: number;
  }): void {
    const targetSearch = changes.search !== undefined ? changes.search : this.search();
    const targetStatus = changes.status !== undefined ? changes.status : this.status();
    const targetPage = changes.page !== undefined ? changes.page : this.page();
    const targetPageSize = changes.pageSize !== undefined ? changes.pageSize : this.pageSize();

    const queryParams: Record<string, string | number | null> = {
      search: targetSearch.trim() || null,
      status: targetStatus !== 'all' ? targetStatus : null,
      page: targetPage > 1 ? targetPage : null,
      pageSize: targetPageSize !== 25 ? targetPageSize : null,
    };

    this.router.navigate([], {
      queryParams,
    });
  }

  private handleQueryParams(params: Record<string, string | undefined>): void {
    const rawSearch = (params['search'] ?? '').trim();

    const rawStatus = params['status'] as StatusFilter;
    const validStatuses: StatusFilter[] = ['all', 'unregistered', 'partial', 'registered'];
    const nextStatus: StatusFilter = validStatuses.includes(rawStatus) ? rawStatus : 'all';

    const rawPage = parseInt(params['page'] ?? '1', 10);
    const nextPage = Number.isFinite(rawPage) && rawPage > 0 ? rawPage : 1;

    const rawPageSize = parseInt(params['pageSize'] ?? '25', 10);
    const validPageSizes = [10, 25, 50, 100];
    const nextPageSize = validPageSizes.includes(rawPageSize) ? rawPageSize : 25;

    const changed =
      !this.initialized ||
      this.search() !== rawSearch ||
      this.status() !== nextStatus ||
      this.page() !== nextPage ||
      this.pageSize() !== nextPageSize;

    if (!changed) {
      return;
    }

    this.initialized = true;
    this.search.set(rawSearch);
    this.status.set(nextStatus);
    this.page.set(nextPage);
    this.pageSize.set(nextPageSize);

    const hasRedundantDefaults =
      (params['search'] !== undefined && !rawSearch) ||
      params['status'] === 'all' ||
      params['page'] === '1' ||
      params['pageSize'] === '25';

    if (hasRedundantDefaults) {
      this.router.navigate([], {
        queryParams: {
          search: rawSearch || null,
          status: nextStatus !== 'all' ? nextStatus : null,
          page: nextPage > 1 ? nextPage : null,
          pageSize: nextPageSize !== 25 ? nextPageSize : null,
        },
        replaceUrl: true,
      });
    }

    this.loadEntries();
  }
}
