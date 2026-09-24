import { Component, ElementRef, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideChevronRight, LucideInbox } from '@lucide/angular';
import {
  LedgerEntryCodeDto,
  LedgerEntrySummaryDto,
  RegisteredAssetDto,
} from '../../models/ledger-entry.model';
import { LedgerDragSelectionService } from '../../services/ledger-drag-selection.service';
import { LedgerEntryService } from '../../services/ledger-entry.service';
import { LedgerExpansionService } from '../../services/ledger-expansion.service';
import { LedgerSelectionService } from '../../services/ledger-selection.service';

@Component({
  selector: 'app-ledger-table',
  imports: [CommonModule, LucideChevronRight, LucideInbox],
  templateUrl: './ledger-table.html',
})
export class LedgerTable {
  readonly ledgerService = inject(LedgerEntryService);
  readonly selection = inject(LedgerSelectionService);
  readonly expansion = inject(LedgerExpansionService);
  readonly drag = inject(LedgerDragSelectionService);

  private readonly elRef = inject(ElementRef);

  // Template signal accessors
  get expandedRows() {
    return this.expansion.expandedRows;
  }
  get expandedCodes() {
    return this.expansion.expandedCodes;
  }
  get detailStates() {
    return this.expansion.detailStates;
  }
  get selectedAssetIds() {
    return this.selection.selectedAssetIds;
  }

  isAssetChecked(assetId: string): boolean {
    return this.selection.isAssetChecked(assetId);
  }

  isFocused(id: string): boolean {
    return this.ledgerService.selectedId() === id;
  }

  remainingCodesTooltip(item: LedgerEntrySummaryDto): string {
    return item.codes
      .slice(1)
      .map((code) => code.code)
      .join(', ');
  }

  isCodeChecked(entryId: string, code: LedgerEntryCodeDto): boolean {
    return this.selection.isCodeChecked(entryId, code);
  }

  isCodeIndeterminate(entryId: string, code: LedgerEntryCodeDto): boolean {
    return this.selection.isCodeIndeterminate(entryId, code);
  }

  isRowChecked(entry: LedgerEntrySummaryDto): boolean {
    return this.selection.isRowChecked(entry);
  }

  isRowPartiallyChecked(entry: LedgerEntrySummaryDto): boolean {
    return this.selection.isRowPartiallyChecked(entry);
  }

  areAllVisibleRowsChecked(): boolean {
    return this.selection.areAllVisibleRowsChecked(this.ledgerService.entries());
  }

  areAnyVisibleRowsChecked(): boolean {
    return this.selection.areAnyVisibleRowsChecked(this.ledgerService.entries());
  }

  setAssetChecked(entryId: string, codeId: string, assetId: string, checked: boolean): void {
    this.selection.setAssetChecked(entryId, codeId, assetId, checked);
  }

  onAssetCheck(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    assetId: string,
    event: Event,
  ): void {
    if (this.drag.suppressSelectionClick) {
      (event.target as HTMLInputElement).checked = this.isAssetChecked(assetId);
      return;
    }
    const checked = (event.target as HTMLInputElement).checked;
    this.setAssetChecked(item.id, code.id, assetId, checked);
  }

  onAssetCheckboxClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.drag.suppressSelectionClick) event.preventDefault();
  }

  onAssetRowClick(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    assetId: string,
    event: MouseEvent,
  ): void {
    if (this.drag.suppressSelectionClick || window.getSelection()?.toString()) {
      event.preventDefault();
      return;
    }
    this.setAssetChecked(item.id, code.id, assetId, !this.isAssetChecked(assetId));
  }

  setCodeChecked(item: LedgerEntrySummaryDto, code: LedgerEntryCodeDto, checked: boolean): void {
    this.selection.setCodeChecked(item, code, checked);
  }

  onCodeCheck(item: LedgerEntrySummaryDto, code: LedgerEntryCodeDto, event: Event): void {
    if (this.drag.suppressSelectionClick) {
      (event.target as HTMLInputElement).checked = this.isCodeChecked(item.id, code);
      return;
    }
    const checked = (event.target as HTMLInputElement).checked;
    this.setCodeChecked(item, code, checked);
  }

  setRowChecked(item: LedgerEntrySummaryDto, checked: boolean): void {
    this.selection.setRowChecked(item, checked);
  }

  onRowCheck(item: LedgerEntrySummaryDto, event: Event): void {
    if (this.drag.suppressSelectionClick) {
      (event.target as HTMLInputElement).checked = this.isRowChecked(item);
      return;
    }
    const checked = (event.target as HTMLInputElement).checked;
    this.setRowChecked(item, checked);
  }

  onVisibleRowsCheck(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.selection.setAllVisibleRowsChecked(this.ledgerService.entries(), checked);
  }

  onRowCheckboxClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.drag.suppressSelectionClick) event.preventDefault();
  }

  onLedgerRowClick(item: LedgerEntrySummaryDto, event: MouseEvent): void {
    if (this.drag.suppressSelectionClick || window.getSelection()?.toString()) {
      event.preventDefault();
      return;
    }
    this.toggleRow(item);
  }

  onRowPointerDown(item: LedgerEntrySummaryDto, event: PointerEvent): void {
    this.drag.onRowPointerDown(item, event);
  }

  onRowPointerEnter(item: LedgerEntrySummaryDto, event: PointerEvent): void {
    this.drag.onRowPointerEnter(item, event);
  }

  @HostListener('window:pointerup')
  endSelectionDrag(): void {
    this.drag.endDrag();
  }

  @HostListener('window:pointercancel')
  cancelSelectionDrag(): void {
    this.drag.cancelDrag();
  }

  onCodeCheckboxClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.drag.suppressSelectionClick) event.preventDefault();
  }

  onCodeRowClick(entryId: string, code: LedgerEntryCodeDto, event: MouseEvent): void {
    if (this.drag.suppressSelectionClick || window.getSelection()?.toString()) {
      event.preventDefault();
      return;
    }
    if (code.groupRegisteredAssetCount <= 0) {
      const entry = this.ledgerService.entries().find((e) => e.id === entryId);
      if (entry) {
        this.setCodeChecked(entry, code, !this.isCodeChecked(entryId, code));
      }
      return;
    }
    this.toggleCode(entryId, code.id);
  }

  onCodePointerDown(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    event: PointerEvent,
  ): void {
    this.drag.onCodePointerDown(item, code, event);
  }

  onCodePointerEnter(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    event: PointerEvent,
  ): void {
    this.drag.onCodePointerEnter(item, code, event);
  }

  onAssetPointerDown(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    assetId: string,
    event: PointerEvent,
  ): void {
    this.drag.onAssetPointerDown(item, code, assetId, event);
  }

  onAssetPointerEnter(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    assetId: string,
    event: PointerEvent,
  ): void {
    this.drag.onAssetPointerEnter(item, code, assetId, event);
  }

  isRowExpanded(id: string): boolean {
    return this.expansion.isRowExpanded(id);
  }

  areAllVisibleRowsExpanded(): boolean {
    return this.expansion.areAllVisibleRowsExpanded(this.ledgerService.entries());
  }

  toggleVisibleRows(): void {
    this.expansion.toggleVisibleRows(this.ledgerService.entries());
  }

  toggleRow(item: LedgerEntrySummaryDto): void {
    this.ledgerService.selectEntry(item.id);
    this.expansion.toggleRow(item.id);
  }

  loadDetail(id: string): void {
    this.expansion.loadDetail(id);
  }

  isCodeExpanded(entryId: string, codeId: string): boolean {
    return this.expansion.isCodeExpanded(entryId, codeId);
  }

  toggleCode(entryId: string, codeId: string): void {
    const entry = this.ledgerService.entries().find((e) => e.id === entryId);
    const code = entry?.codes.find((c) => c.id === codeId);
    if (code && code.groupRegisteredAssetCount <= 0) return;
    this.expansion.toggleCode(entryId, codeId, code?.groupRegisteredAssetCount ?? 0);
  }

  assetsFor(entryId: string, codeId: string): RegisteredAssetDto[] {
    return this.expansion.assetsFor(entryId, codeId);
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
