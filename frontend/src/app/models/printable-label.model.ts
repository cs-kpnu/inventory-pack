export interface PrintableLabelItem {
  assetId: string;
  payload: string;
  name: string;
  code: string;
  mvo: string | null;
  allocatedAt: string;
  subaccount: string;
  qrDataUrl?: string;
}
