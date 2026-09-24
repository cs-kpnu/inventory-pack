import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface AssetImportResponse {
  importedLedgerEntries: number;
  rejectedRows: number;
}

@Injectable({
  providedIn: 'root',
})
export class AssetImportService {
  private readonly http = inject(HttpClient);

  importExcel(file: File): Observable<AssetImportResponse> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<AssetImportResponse>('/api/assets/import', formData);
  }
}
