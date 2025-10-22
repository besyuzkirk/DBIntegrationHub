import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Not: localStorage server-side'da çalışmadığı için middleware ile auth kontrolü yapamıyoruz.
// Auth koruması client-side'da useAuth hook'u ve useEffect'ler ile sağlanıyor.
// Bu middleware sadece gelecekteki cookie-based auth için hazır tutulmuştur.

export function middleware(request: NextRequest) {
  // Şimdilik sadece request'i geçir
  // Client-side protection layout ve page'lerde yapılıyor
  return NextResponse.next();
}

export const config = {
  matcher: ['/dashboard/:path*', '/login'],
};

