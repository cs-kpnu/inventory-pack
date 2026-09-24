import { CommonModule } from '@angular/common';
import { Component, computed, HostListener, inject, input, output, signal } from '@angular/core';
import { from } from 'rxjs';
import { concatMap, toArray } from 'rxjs/operators';
import {
  LucideAlertCircle,
  LucideCheck,
  LucidePlus,
  LucideRotateCw,
  LucideX,
} from '@lucide/angular';
import {
  LedgerEntryCodeDto,
  LedgerEntrySummaryDto,
  RegisterAssetsRequest,
} from '../../models/ledger-entry.model';
import { AssetRegistrationService } from '../../services/asset-registration.service';

@Component({
  selector: 'app-asset-registration-dialog',
  imports: [CommonModule, LucidePlus, LucideX, LucideCheck, LucideAlertCircle, LucideRotateCw],
  templateUrl: './asset-registration-dialog.html',
})
export class AssetRegistrationDialog {
  readonly entries = input.required<LedgerEntrySummaryDto[]>();
  readonly selectedCodeIdsByEntry = input.required<Map<string, Set<string>>>();
  readonly unlistedCodeCount = input(0);
  readonly dismiss = output<void>();
  readonly registered = output<void>();

  readonly quantities = signal<Map<string, string>>(new Map());
  readonly submitting = signal(false);
  readonly confirming = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly registeredAssetCount = signal<number | null>(null);
  readonly registrationService = inject(AssetRegistrationService);

  readonly groups = computed(() => {
    const selection = this.selectedCodeIdsByEntry();

    return this.entries()
      .map((entry) => ({
        entry,
        codes: entry.codes.filter((code) => selection.get(entry.id)?.has(code.id)),
      }))
      .filter((group) => group.codes.length > 0);
  });

  readonly visibleCodeCount = computed(() =>
    this.groups().reduce((count, group) => count + group.codes.length, 0),
  );

  readonly invalidQuantityCount = computed(() =>
    this.groups().reduce(
      (count, group) =>
        count + group.codes.filter((code) => !this.isQuantityValid(group.entry.id, code.id)).length,
      0,
    ),
  );

  readonly totalQuantity = computed(() =>
    this.groups().reduce(
      (total, group) =>
        total +
        group.codes.reduce((groupTotal, code) => {
          const quantity = this.quantityFor(group.entry.id, code.id);
          return (
            groupTotal + (this.isQuantityValid(group.entry.id, code.id) ? Number(quantity) : 0)
          );
        }, 0),
      0,
    ),
  );

  readonly canRegister = computed(
    () =>
      !this.submitting() &&
      this.registeredAssetCount() === null &&
      this.unlistedCodeCount() === 0 &&
      this.visibleCodeCount() > 0 &&
      this.invalidQuantityCount() === 0,
  );

  entryQuantityTotal(group: { entry: LedgerEntrySummaryDto; codes: LedgerEntryCodeDto[] }): number {
    return group.codes.reduce((sum, code) => {
      const quantity = this.quantityFor(group.entry.id, code.id);
      return sum + (this.isQuantityValid(group.entry.id, code.id) ? Number(quantity) : 0);
    }, 0);
  }

  formatAssetCount(count: number): string {
    const mod10 = count % 10;
    const mod100 = count % 100;
    if (mod100 >= 11 && mod100 <= 19) {
      return `${count} об'єктів`;
    }
    if (mod10 === 1) {
      return `${count} об'єкт`;
    }
    if (mod10 >= 2 && mod10 <= 4) {
      return `${count} об'єкти`;
    }
    return `${count} об'єктів`;
  }

  quantityFor(entryId: string, codeId: string): string {
    const selectedQuantity = this.quantities().get(this.quantityKey(entryId, codeId));
    if (selectedQuantity !== undefined) return selectedQuantity;

    const draftQuantity = this.registrationService.quantityFor(entryId, codeId);
    if (draftQuantity !== undefined) return draftQuantity;

    const entry = this.entries().find((candidate) => candidate.id === entryId);
    if (!entry || !this.isPieceUnit(entry.unit)) return '';

    const ledgerQuantity = entry.quantity;
    if (!Number.isInteger(ledgerQuantity) || ledgerQuantity < 1) return '';
    if (entry.codes.length === 1 && ledgerQuantity <= 50) return String(ledgerQuantity);
    if (ledgerQuantity === entry.codes.length) return '1';
    return '';
  }

  isQuantityValid(entryId: string, codeId: string): boolean {
    const value = this.quantityFor(entryId, codeId).trim();
    if (value === '') return false;

    const quantity = Number(value);
    return Number.isInteger(quantity) && quantity >= 1 && quantity <= 50;
  }

  updateQuantity(entryId: string, codeId: string, event: Event): void {
    this.confirming.set(false);
    const input = event.target as HTMLInputElement;
    this.registrationService.setQuantity(entryId, codeId, input.value);
    this.quantities.update((quantities) => {
      const next = new Map(quantities);
      next.set(this.quantityKey(entryId, codeId), input.value);
      return next;
    });
  }

  promptRegister(): void {
    if (this.canRegister()) {
      this.confirming.set(true);
    }
  }

  cancelConfirmation(): void {
    this.confirming.set(false);
  }

  register(): void {
    if (!this.canRegister()) return;

    this.confirming.set(false);
    const requests: RegisterAssetsRequest[] = [];
    for (const group of this.groups()) {
      for (const code of group.codes) {
        const count = Number(this.quantityFor(group.entry.id, code.id));
        requests.push({
          ledgerEntryId: group.entry.id,
          codeGroupId: code.id,
          requestId: this.registrationService.requestIdFor(group.entry.id, code.id, count),
          count,
        });
      }
    }

    if (requests.length === 0) return;

    this.registrationService.markAttempted(requests);
    this.submitting.set(true);
    this.errorMessage.set(null);

    from(requests)
      .pipe(
        concatMap((request) => this.registrationService.register(request)),
        toArray(),
      )
      .subscribe({
        next: (results) => {
          const registeredCount = results.reduce((count, assets) => count + assets.length, 0);
          this.registrationService.complete(requests);
          this.registeredAssetCount.set(registeredCount);
          this.submitting.set(false);
          this.registered.emit();
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.registrationError(error));
          this.submitting.set(false);
        },
      });
  }

  @HostListener('window:keydown.escape')
  closeOnEscape(): void {
    if (this.confirming()) {
      this.confirming.set(false);
      return;
    }
    this.requestDismiss();
  }

  requestDismiss(): void {
    if (!this.submitting()) this.dismiss.emit();
  }

  private isPieceUnit(unit: string | null | undefined): boolean {
    const normalized = unit?.trim().toLowerCase();
    return normalized === 'шт' || normalized === 'шт.';
  }

  private quantityKey(entryId: string, codeId: string): string {
    return `${entryId}:${codeId}`;
  }

  private registrationError(error: unknown): string {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      const body = (error as { error?: unknown }).error;
      if (typeof body === 'object' && body !== null) {
        const message =
          (body as { error?: unknown; message?: unknown }).error ??
          (body as { message?: unknown }).message;
        if (typeof message === 'string') return message;
      }
    }

    if (error instanceof Error && error.message) return error.message;
    return 'Не вдалося зареєструвати об’єкти. Перевірте з’єднання та повторіть спробу.';
  }
}
