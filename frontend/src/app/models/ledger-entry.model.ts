export interface LedgerEntryCodeDto {
  id: string;
  code: string;
  groupRegisteredAssetCount: number;
  mvoRegisteredAssetCount: number;
  unassignedAssetCount: number;
  sourceRowCount: number;
}

export interface LedgerEntrySummaryDto {
  id: string;
  sourceRowNumber: number;
  name: string;
  quantity: number;
  unit: string | null;
  mvo: string | null;
  subaccount: string;
  codes: LedgerEntryCodeDto[];
}

export interface LedgerEntryListResponse {
  items: LedgerEntrySummaryDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasRejectedRows: boolean;
}

export interface RegisteredAssetDto {
  id: string;
  payload: string;
  allocatedAt: string;
  registeredForMvo: string | null;
}

export interface LedgerEntryDetailCodeDto {
  id: string;
  code: string;
  groupRegisteredAssetCount: number;
  mvoRegisteredAssetCount: number;
  unassignedAssetCount: number;
  sourceRowCount: number;
  assets: RegisteredAssetDto[];
}

export interface LedgerEntryDetailDto {
  id: string;
  sourceRowNumber: number;
  sourceTitle: string;
  name: string;
  quantity: number;
  unit: string | null;
  mvo: string | null;
  subaccount: string;
  codes: LedgerEntryDetailCodeDto[];
}

export interface RegisterAssetsRequest {
  ledgerEntryId: string;
  codeGroupId: string;
  requestId: string;
  count: number;
}

export type StatusFilter = 'all' | 'unregistered' | 'partial' | 'registered';
