import { computed, inject, Injectable, signal } from '@angular/core';
import * as QRCode from 'qrcode';
import { PrintableLabelItem } from '../models/printable-label.model';
import { LedgerExpansionService } from './ledger-expansion.service';
import { LedgerSelectionService } from './ledger-selection.service';

@Injectable({
  providedIn: 'root',
})
export class AssetPrintService {
  private readonly selection = inject(LedgerSelectionService);
  private readonly expansion = inject(LedgerExpansionService);

  private readonly qrCache = new Map<string, string>();
  private readonly qrVersion = signal(0);

  /**
   * Computed list of all currently selected assets with their ledger & code metadata.
   */
  readonly selectedLabels = computed<PrintableLabelItem[]>(() => {
    // Read qrVersion to re-evaluate when new QR codes are generated
    this.qrVersion();

    const selectedAssetIds = this.selection.selectedAssetIds();
    if (selectedAssetIds.size === 0) return [];

    const details = this.expansion.detailStates();
    const result: PrintableLabelItem[] = [];

    for (const [, state] of details) {
      if (!state.entry) continue;
      const entry = state.entry;
      for (const code of entry.codes) {
        for (const asset of code.assets) {
          if (selectedAssetIds.has(asset.id)) {
            result.push({
              assetId: asset.id,
              payload: asset.payload,
              name: entry.name,
              code: code.code,
              mvo: asset.registeredForMvo ?? entry.mvo,
              allocatedAt: asset.allocatedAt,
              subaccount: entry.subaccount,
              qrDataUrl: this.qrCache.get(asset.payload),
            });
          }
        }
      }
    }

    return result;
  });

  /**
   * Ensures all selected entries have their detail loaded in expansion service.
   */
  ensureSelectedDetailsLoaded(): void {
    const selectedCodes = this.selection.selectedCodesByEntry();
    for (const entryId of selectedCodes.keys()) {
      if (!this.expansion.detailStates().has(entryId)) {
        this.expansion.loadDetail(entryId);
      }
    }
  }

  /**
   * Generates or retrieves from cache the QR code data URL for a given payload.
   */
  async generateQr(payload: string): Promise<string> {
    const cached = this.qrCache.get(payload);
    if (cached) return cached;

    const dataUrl = await QRCode.toDataURL(payload, {
      errorCorrectionLevel: 'M',
      margin: 1,
      width: 256,
      color: {
        dark: '#000000',
        light: '#ffffff',
      },
    });

    this.qrCache.set(payload, dataUrl);
    this.qrVersion.update((v) => v + 1);
    return dataUrl;
  }

  /**
   * Generates QR codes for all given labels concurrently.
   */
  async generateAllQrs(labels: PrintableLabelItem[]): Promise<PrintableLabelItem[]> {
    const updated = await Promise.all(
      labels.map(async (item) => {
        const qrDataUrl = await this.generateQr(item.payload);
        return {
          ...item,
          qrDataUrl,
        };
      }),
    );
    return updated;
  }
}
