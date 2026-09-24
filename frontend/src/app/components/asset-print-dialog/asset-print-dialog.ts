import { CommonModule } from '@angular/common';
import { Component, effect, HostListener, inject, output, signal } from '@angular/core';
import { LucidePrinter, LucideRotateCw, LucideX } from '@lucide/angular';
import { PrintableLabelItem } from '../../models/printable-label.model';
import { AssetPrintService } from '../../services/asset-print.service';
import { LedgerSelectionService } from '../../services/ledger-selection.service';

@Component({
  selector: 'app-asset-print-dialog',
  imports: [CommonModule, LucidePrinter, LucideX, LucideRotateCw],
  templateUrl: './asset-print-dialog.html',
  styleUrl: './asset-print-dialog.css',
})
export class AssetPrintDialog {
  readonly dismiss = output<void>();

  readonly printService = inject(AssetPrintService);
  readonly selectionService = inject(LedgerSelectionService);

  readonly loading = signal(true);
  readonly preparedLabels = signal<PrintableLabelItem[]>([]);
  readonly showMvo = signal(false);
  readonly labelWidthMm = signal(58);
  readonly labelHeightMm = signal(40);

  constructor() {
    this.printService.ensureSelectedDetailsLoaded();

    effect(() => {
      const rawLabels = this.printService.selectedLabels();
      const expectedCount = this.selectionService.selectedAssetCount();

      if (rawLabels.length === 0 && expectedCount > 0) {
        this.loading.set(true);
        return;
      }

      this.loading.set(true);
      this.printService.generateAllQrs(rawLabels).then((withQrs) => {
        this.preparedLabels.set(withQrs);
        this.loading.set(false);
      });
    });
  }

  toggleMvo(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.showMvo.set(input.checked);
  }

  setLabelWidth(event: Event): void {
    const value = this.readDimension(event);
    if (value !== null) this.labelWidthMm.set(value);
  }

  setLabelHeight(event: Event): void {
    const value = this.readDimension(event);
    if (value !== null) this.labelHeightMm.set(value);
  }

  restoreLabelWidth(event: Event): void {
    this.restoreDimensionInput(event, this.labelWidthMm());
  }

  restoreLabelHeight(event: Event): void {
    this.restoreDimensionInput(event, this.labelHeightMm());
  }

  resetLabelDimensions(): void {
    this.labelWidthMm.set(58);
    this.labelHeightMm.set(40);
  }

  print(): void {
    const width = this.labelWidthMm();
    const height = this.labelHeightMm();

    document.documentElement.style.setProperty('--thermal-label-width', `${width}mm`);
    document.documentElement.style.setProperty('--thermal-label-height', `${height}mm`);

    let pageSizeStyle = document.getElementById(
      'thermal-label-page-size',
    ) as HTMLStyleElement | null;
    if (!pageSizeStyle) {
      pageSizeStyle = document.createElement('style');
      pageSizeStyle.id = 'thermal-label-page-size';
      document.head.append(pageSizeStyle);
    }
    pageSizeStyle.textContent = `@page { size: ${width}mm ${height}mm; margin: 0; }`;

    window.print();
  }

  @HostListener('window:keydown.escape')
  closeOnEscape(): void {
    this.requestDismiss();
  }

  requestDismiss(): void {
    this.dismiss.emit();
  }

  private readDimension(event: Event): number | null {
    const value = (event.target as HTMLInputElement).valueAsNumber;
    if (!Number.isFinite(value) || value < 1 || value > 300) return null;
    return Math.round(value * 2) / 2;
  }

  private restoreDimensionInput(event: Event, value: number): void {
    const input = event.target as HTMLInputElement;
    if (!input.validity.valid) input.value = String(value);
  }
}
