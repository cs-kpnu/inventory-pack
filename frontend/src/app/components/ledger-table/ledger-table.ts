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

type SelectionTarget =
  | { kind: 'row'; key: string; entry: LedgerEntrySummaryDto }
  | { kind: 'code'; key: string; entryId: string; codeId: string };

@Component({
  selector: 'app-ledger-table',
  imports: [CommonModule, LucideChevronRight, LucideInbox],
  templateUrl: './ledger-table.html',
})
export class LedgerTable {
  readonly ledgerService = inject(LedgerEntryService);
  readonly expandedRows = signal<Set<string>>(new Set());
  readonly expandedCodes = signal<Set<string>>(new Set());
  readonly detailStates = signal<Map<string, DetailState>>(new Map());
  private readonly elRef = inject(ElementRef);
  private selectionDrag: {
    startKey: string;
    checked: boolean;
    visited: Set<string>;
  } | null = null;
  private suppressSelectionClick = false;

  isFocused(id: string): boolean {
    return this.ledgerService.selectedId() === id;
  }

  remainingCodesTooltip(item: LedgerEntrySummaryDto): string {
    return item.codes
      .slice(1)
      .map((code) => code.code)
      .join(', ');
  }

  onRowCheck(item: LedgerEntrySummaryDto, event: Event): void {
    if (this.suppressSelectionClick) {
      (event.target as HTMLInputElement).checked = this.ledgerService.isRowChecked(item);
      return;
    }
    this.ledgerService.setRowChecked(item, (event.target as HTMLInputElement).checked);
  }

  areAllVisibleRowsChecked(): boolean {
    const entries = this.ledgerService.entries();
    return entries.length > 0 && entries.every((entry) => this.ledgerService.isRowChecked(entry));
  }

  areAnyVisibleRowsChecked(): boolean {
    return this.ledgerService
      .entries()
      .some((entry) =>
        entry.codes.some((code) => this.ledgerService.isCodeChecked(entry.id, code.id)),
      );
  }

  onVisibleRowsCheck(event: Event): void {
    this.ledgerService.setRowsChecked(
      this.ledgerService.entries(),
      (event.target as HTMLInputElement).checked,
    );
  }

  onRowCheckboxClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.suppressSelectionClick) event.preventDefault();
  }

  onLedgerRowClick(item: LedgerEntrySummaryDto, event: MouseEvent): void {
    if (this.suppressSelectionClick || window.getSelection()?.toString()) {
      event.preventDefault();
      return;
    }
    this.toggleRow(item);
  }

  onRowPointerDown(item: LedgerEntrySummaryDto, event: PointerEvent): void {
    if (event.pointerType !== 'mouse' || event.button !== 0) return;
    const target = event.target as Element;
    const checkboxCell = (event.currentTarget as HTMLTableRowElement).cells[0];
    if (target.closest('td') !== checkboxCell) return;
    this.beginSelectionDrag(`row:${item.id}`, !this.ledgerService.isRowChecked(item));
  }

  onRowPointerEnter(item: LedgerEntrySummaryDto, event: PointerEvent): void {
    this.extendSelectionDrag(`row:${item.id}`, event);
  }

  @HostListener('window:pointerup')
  endSelectionDrag(): void {
    this.selectionDrag = null;
    if (this.suppressSelectionClick) {
      setTimeout(() => (this.suppressSelectionClick = false));
    }
  }

  @HostListener('window:pointercancel')
  cancelSelectionDrag(): void {
    this.endSelectionDrag();
  }

  onCodeCheck(entryId: string, codeId: string, event: Event): void {
    if (this.suppressSelectionClick) {
      (event.target as HTMLInputElement).checked = this.ledgerService.isCodeChecked(
        entryId,
        codeId,
      );
      return;
    }
    this.ledgerService.setCodeChecked(entryId, codeId, (event.target as HTMLInputElement).checked);
  }

  onCodeCheckboxClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.suppressSelectionClick) event.preventDefault();
  }

  onCodeRowClick(entryId: string, codeId: string, event: MouseEvent): void {
    if (this.suppressSelectionClick || window.getSelection()?.toString()) {
      event.preventDefault();
      return;
    }
    this.ledgerService.setCodeChecked(
      entryId,
      codeId,
      !this.ledgerService.isCodeChecked(entryId, codeId),
    );
  }

  onCodePointerDown(entryId: string, codeId: string, event: PointerEvent): void {
    if (event.pointerType !== 'mouse' || event.button !== 0) return;
    const checkboxCell = (event.currentTarget as HTMLTableRowElement).cells[1];
    if ((event.target as Element).closest('td') !== checkboxCell) return;
    this.beginSelectionDrag(
      `code:${entryId}:${codeId}`,
      !this.ledgerService.isCodeChecked(entryId, codeId),
    );
  }

  onCodePointerEnter(entryId: string, codeId: string, event: PointerEvent): void {
    this.extendSelectionDrag(`code:${entryId}:${codeId}`, event);
  }

  isRowExpanded(id: string): boolean {
    return this.expandedRows().has(id);
  }

  areAllVisibleRowsExpanded(): boolean {
    const entries = this.ledgerService.entries();
    return entries.length > 0 && entries.every((entry) => this.expandedRows().has(entry.id));
  }

  toggleVisibleRows(): void {
    const entries = this.ledgerService.entries();
    if (entries.length === 0) return;

    const collapse = entries.every((entry) => this.expandedRows().has(entry.id));
    this.expandedRows.update((rows) => {
      const next = new Set(rows);
      for (const entry of entries) {
        if (collapse) next.delete(entry.id);
        else next.add(entry.id);
      }
      return next;
    });
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

  private beginSelectionDrag(startKey: string, checked: boolean): void {
    this.selectionDrag = { startKey, checked, visited: new Set<string>() };
  }

  private extendSelectionDrag(targetKey: string, event: PointerEvent): void {
    const drag = this.selectionDrag;
    if (!drag || (event.buttons & 1) === 0) return;

    const targets = this.visibleSelectionTargets();
    const startIndex = targets.findIndex((target) => target.key === drag.startKey);
    const endIndex = targets.findIndex((target) => target.key === targetKey);
    if (startIndex < 0 || endIndex < 0 || startIndex === endIndex) return;

    this.suppressSelectionClick = true;
    const newlyVisited = targets
      .slice(Math.min(startIndex, endIndex), Math.max(startIndex, endIndex) + 1)
      .filter((target) => !drag.visited.has(target.key));

    for (const target of newlyVisited) drag.visited.add(target.key);

    this.ledgerService.setRowsChecked(
      newlyVisited
        .filter(
          (target): target is Extract<SelectionTarget, { kind: 'row' }> => target.kind === 'row',
        )
        .map((target) => target.entry),
      drag.checked,
    );

    this.ledgerService.setCodesChecked(
      newlyVisited
        .filter(
          (target): target is Extract<SelectionTarget, { kind: 'code' }> => target.kind === 'code',
        )
        .map((target) => ({ entryId: target.entryId, codeId: target.codeId })),
      drag.checked,
    );
  }

  private visibleSelectionTargets(): SelectionTarget[] {
    const targets: SelectionTarget[] = [];
    for (const entry of this.ledgerService.entries()) {
      targets.push({ kind: 'row', key: `row:${entry.id}`, entry });
      if (this.isRowExpanded(entry.id)) {
        for (const code of entry.codes) {
          targets.push({
            kind: 'code',
            key: `code:${entry.id}:${code.id}`,
            entryId: entry.id,
            codeId: code.id,
          });
        }
      }
    }
    return targets;
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
