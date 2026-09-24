import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { LucideUpload, LucideX } from '@lucide/angular';
import { LedgerEntryService } from './services/ledger-entry.service';
import { AssetImportService } from './services/asset-import.service';
import { LedgerToolbar } from './components/ledger-toolbar/ledger-toolbar';
import { LedgerTable } from './components/ledger-table/ledger-table';
import { LedgerPagination } from './components/ledger-pagination/ledger-pagination';

@Component({
  selector: 'app-root',
  imports: [CommonModule, LucideUpload, LucideX, LedgerToolbar, LedgerTable, LedgerPagination],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  readonly ledgerService = inject(LedgerEntryService);
  readonly importingExcel = signal(false);
  readonly importNotice = signal<{
    kind: 'success' | 'warning' | 'error';
    message: string;
  } | null>(null);
  private readonly assetImportService = inject(AssetImportService);

  uploadExcel(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';

    if (!file) return;

    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      this.importNotice.set({
        kind: 'error',
        message: 'Виберіть файл Excel у форматі .xlsx.',
      });
      return;
    }

    this.importingExcel.set(true);
    this.importNotice.set(null);

    this.assetImportService.importExcel(file).subscribe({
      next: (result) => {
        this.importingExcel.set(false);
        this.ledgerService.resetAfterImport();

        const kind = result.rejectedRows > 0 ? 'warning' : 'success';
        this.importNotice.set({
          kind,
          message: `Імпортовано рядків: ${result.importedLedgerEntries}. Відхилено: ${result.rejectedRows}.`,
        });
      },
      error: (error: unknown) => {
        this.importingExcel.set(false);
        this.importNotice.set({
          kind: 'error',
          message: this.getImportErrorMessage(error),
        });
      },
    });
  }

  dismissImportNotice(): void {
    this.importNotice.set(null);
  }

  private getImportErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Не вдалося імпортувати файл.';
    }

    if (error.status === 409) {
      return 'Дані вже імпортовано. Повторний імпорт не підтримується.';
    }

    if (error.status === 415) {
      return 'Сервер підтримує лише файли Excel у форматі .xlsx.';
    }

    if (error.status === 0) {
      return 'Не вдалося зв’язатися із сервером. Перевірте з’єднання та спробуйте ще раз.';
    }

    const detail = error.error?.detail;
    return typeof detail === 'string' && detail.length > 0
      ? detail
      : 'Не вдалося імпортувати файл. Перевірте файл і спробуйте ще раз.';
  }
}
