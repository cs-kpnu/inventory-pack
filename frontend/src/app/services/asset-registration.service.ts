import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterAssetsRequest, RegisteredAssetDto } from '../models/ledger-entry.model';

@Injectable({ providedIn: 'root' })
export class AssetRegistrationService {
  private readonly http = inject(HttpClient);
  private readonly draftQuantities = new Map<string, string>();
  private readonly requestIds = new Map<string, Map<number, string>>();
  private readonly attemptedKeys = new Set<string>();

  register(request: RegisterAssetsRequest): Observable<RegisteredAssetDto[]> {
    return this.http.post<RegisteredAssetDto[]>('/api/assets', request);
  }

  quantityFor(ledgerEntryId: string, codeGroupId: string): string | undefined {
    return this.draftQuantities.get(this.key(ledgerEntryId, codeGroupId));
  }

  setQuantity(ledgerEntryId: string, codeGroupId: string, quantity: string): void {
    this.draftQuantities.set(this.key(ledgerEntryId, codeGroupId), quantity);
  }

  hasAttempted(ledgerEntryId: string, codeGroupId: string): boolean {
    return this.attemptedKeys.has(this.key(ledgerEntryId, codeGroupId));
  }

  markAttempted(requests: readonly RegisterAssetsRequest[]): void {
    for (const request of requests) {
      this.attemptedKeys.add(this.key(request.ledgerEntryId, request.codeGroupId));
    }
  }

  requestIdFor(ledgerEntryId: string, codeGroupId: string, count: number): string {
    const key = this.key(ledgerEntryId, codeGroupId);
    const idsByCount = this.requestIds.get(key) ?? new Map<number, string>();
    const existingId = idsByCount.get(count);
    if (existingId) return existingId;

    const requestId = crypto.randomUUID();
    idsByCount.set(count, requestId);
    this.requestIds.set(key, idsByCount);
    return requestId;
  }

  complete(requests: readonly RegisterAssetsRequest[]): void {
    for (const request of requests) {
      const key = this.key(request.ledgerEntryId, request.codeGroupId);
      this.draftQuantities.delete(key);
      this.requestIds.delete(key);
      this.attemptedKeys.delete(key);
    }
  }

  private key(ledgerEntryId: string, codeGroupId: string): string {
    return `${ledgerEntryId}:${codeGroupId}`;
  }
}
