import { CommonModule } from '@angular/common';
import { Component, computed, effect, HostListener, inject, output, signal } from '@angular/core';
import { LucidePrinter, LucideRotateCw, LucideX } from '@lucide/angular';
import { PrintableLabelItem } from '../../models/printable-label.model';
import { AssetPrintService } from '../../services/asset-print.service';
import { LedgerSelectionService } from '../../services/ledger-selection.service';

const DEFAULT_LABEL_WIDTH_MM = 72;
const DEFAULT_LABEL_HEIGHT_MM = 40;

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
  readonly labelWidthMm = signal(DEFAULT_LABEL_WIDTH_MM);
  readonly labelHeightMm = signal(DEFAULT_LABEL_HEIGHT_MM);
  readonly scaleContent = signal(false);
  readonly contentScale = computed(() =>
    this.scaleContent()
      ? Math.min(
          this.labelWidthMm() / DEFAULT_LABEL_WIDTH_MM,
          this.labelHeightMm() / DEFAULT_LABEL_HEIGHT_MM,
        )
      : 1,
  );

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

  toggleContentScaling(event: Event): void {
    this.scaleContent.set((event.target as HTMLInputElement).checked);
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
    this.labelWidthMm.set(DEFAULT_LABEL_WIDTH_MM);
    this.labelHeightMm.set(DEFAULT_LABEL_HEIGHT_MM);
    this.scaleContent.set(false);
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
