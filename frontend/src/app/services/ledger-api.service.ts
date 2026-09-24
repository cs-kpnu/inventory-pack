import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  LedgerEntryDetailDto,
  LedgerEntryListResponse,
  StatusFilter,
} from '../models/ledger-entry.model';

export interface LedgerQueryParams {
  page: number;
  pageSize: number;
  search?: string;
  status?: StatusFilter;
}

@Injectable({
  providedIn: 'root',
})
export class LedgerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/ledger-entries';

  getEntries(query: LedgerQueryParams): Observable<LedgerEntryListResponse> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search?.trim()) {
      params = params.set('search', query.search.trim());
    }

    if (query.status && query.status !== 'all') {
      params = params.set('status', query.status);
    }

    return this.http.get<LedgerEntryListResponse>(this.baseUrl, { params });
  }

  getEntryDetail(id: string): Observable<LedgerEntryDetailDto> {
    return this.http.get<LedgerEntryDetailDto>(`${this.baseUrl}/${id}`);
  }
}
