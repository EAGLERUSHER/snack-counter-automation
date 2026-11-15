export type OfficeId = 'Oslo' | 'Bengaluru';
export type Currency = 'NOK' | 'INR';
export type RefillStatus =
  | 'ReportedEmpty'
  | 'Assigned'
  | 'Committed'
  | 'Purchased'
  | 'HrPending'
  | 'Reimbursed'
  | 'Reassigned';

export interface OfficeInfo {
  id: OfficeId;
  monthlyBudget: number;
  currency: Currency;
  spentThisMonth: number;
  overBudget: boolean;
}

export interface Employee {
  id: string;
  name: string;
  homeOffice: OfficeId;
  locationStatus: string;
  visitingOffice?: OfficeId | null;
  lastRefillAt?: string | null;
  refillCountThisQuarter: number;
  openRefillCommitmentId?: string | null;
}

export interface Refill {
  id: string;
  officeId: OfficeId;
  status: RefillStatus;
  reportedByEmployeeId: string;
  assignedVolunteerId?: string | null;
  committedByEmployeeId?: string | null;
  createdAt: string;
  assignmentExpiresAt?: string | null;
  purchaseAmount?: number | null;
  purchaseCurrency?: Currency | null;
  hrClaimId?: string | null;
}

export interface ChatMessage {
  target: string;
  text: string;
  at: string;
}

export interface MonthlyReport {
  year: number;
  month: number;
  offices: {
    officeId: OfficeId;
    spent: number;
    budget: number;
    currency: Currency;
    overBudget: boolean;
  }[];
  totalInReportingCurrency: number;
  reportingCurrency: Currency;
}

export interface ErrorResponse {
  message: string;
  code?: string | null;
}