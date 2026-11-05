export type PaginationRequest = {
  page: number;
  pageSize: number;
};

// Зеркалит Shared.SharedKernel.Responses.PaginationResponse<T> (items/...).
export type PaginationResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
