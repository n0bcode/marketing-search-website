export class ResponseAPI<T> {
  constructor(
    public data: T | null,
    public success: boolean = true,
    public message: string = ''
  ) {}
}
