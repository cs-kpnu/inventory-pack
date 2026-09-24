import { inject, Injectable } from '@angular/core';
import {
  LedgerEntryCodeDto,
  LedgerEntrySummaryDto,
  SelectionTarget,
} from '../models/ledger-entry.model';
import { LedgerEntryService } from './ledger-entry.service';
import { LedgerExpansionService } from './ledger-expansion.service';
import { LedgerSelectionService } from './ledger-selection.service';

@Injectable({
  providedIn: 'root',
})
export class LedgerDragSelectionService {
  suppressSelectionClick = false;
  private readonly ledgerService = inject(LedgerEntryService);
  private readonly expansion = inject(LedgerExpansionService);
  private readonly selection = inject(LedgerSelectionService);
  private selectionDrag: {
    startKey: string;
    checked: boolean;
    visited: Set<string>;
  } | null = null;

  beginDrag(startKey: string, checked: boolean): void {
    this.selectionDrag = { startKey, checked, visited: new Set<string>() };
  }

  extendDrag(targetKey: string, event: PointerEvent): void {
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

    for (const target of newlyVisited) {
      if (target.kind === 'row') {
        this.selection.setRowChecked(target.entry, drag.checked);
      } else if (target.kind === 'code') {
        this.selection.setCodeChecked(target.entry, target.code, drag.checked);
      } else if (target.kind === 'asset') {
        this.selection.setAssetChecked(
          target.entry.id,
          target.code.id,
          target.assetId,
          drag.checked,
        );
      }
    }
  }

  endDrag(): void {
    this.selectionDrag = null;
    if (this.suppressSelectionClick) {
      setTimeout(() => (this.suppressSelectionClick = false));
    }
  }

  cancelDrag(): void {
    this.endDrag();
  }

  onRowPointerDown(item: LedgerEntrySummaryDto, event: PointerEvent): void {
    if (event.pointerType !== 'mouse' || event.button !== 0) return;
    const target = event.target as Element;
    const checkboxCell = (event.currentTarget as HTMLTableRowElement).cells[0];
    if (target.closest('td') !== checkboxCell) return;
    this.beginDrag(`row:${item.id}`, !this.selection.isRowChecked(item));
  }

  onRowPointerEnter(item: LedgerEntrySummaryDto, event: PointerEvent): void {
    this.extendDrag(`row:${item.id}`, event);
  }

  onCodePointerDown(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    event: PointerEvent,
  ): void {
    if (event.pointerType !== 'mouse' || event.button !== 0) return;
    const target = event.target as Element;
    if (!target.closest('[data-code-checkbox]')) return;
    this.beginDrag(`code:${item.id}:${code.id}`, !this.selection.isCodeChecked(item.id, code));
  }

  onCodePointerEnter(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    event: PointerEvent,
  ): void {
    this.extendDrag(`code:${item.id}:${code.id}`, event);
  }

  onAssetPointerDown(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    assetId: string,
    event: PointerEvent,
  ): void {
    if (event.pointerType !== 'mouse' || event.button !== 0) return;
    const target = event.target as Element;
    if (!target.closest('[data-asset-checkbox]')) return;
    this.beginDrag(
      `asset:${item.id}:${code.id}:${assetId}`,
      !this.selection.isAssetChecked(assetId),
    );
  }

  onAssetPointerEnter(
    item: LedgerEntrySummaryDto,
    code: LedgerEntryCodeDto,
    assetId: string,
    event: PointerEvent,
  ): void {
    this.extendDrag(`asset:${item.id}:${code.id}:${assetId}`, event);
  }

  visibleSelectionTargets(): SelectionTarget[] {
    const targets: SelectionTarget[] = [];
    for (const entry of this.ledgerService.entries()) {
      targets.push({ kind: 'row', key: `row:${entry.id}`, entry });
      if (this.expansion.isRowExpanded(entry.id)) {
        for (const code of entry.codes) {
          targets.push({
            kind: 'code',
            key: `code:${entry.id}:${code.id}`,
            entry,
            code,
          });
          if (
            code.groupRegisteredAssetCount > 0 &&
            this.expansion.isCodeExpanded(entry.id, code.id)
          ) {
            for (const asset of this.expansion.assetsFor(entry.id, code.id)) {
              targets.push({
                kind: 'asset',
                key: `asset:${entry.id}:${code.id}:${asset.id}`,
                entry,
                code,
                assetId: asset.id,
              });
            }
          }
        }
      }
    }
    return targets;
  }
}
