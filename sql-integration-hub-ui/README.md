# SQL Integration Hub UI

Veritabanı entegrasyonu ve yönetimi için minimalist, Zen estetiğinde tasarlanmış bir Next.js 14 uygulaması.

## ✨ Özellikler

- 🎨 **Zen Minimalist Tasarım** - Notion + Linear + Apple estetiği
- ⚡ **Next.js 14** - App Router ile modern React
- 🎯 **TypeScript** - Tip güvenli geliştirme
- 💅 **TailwindCSS** - Utility-first CSS framework
- 🧩 **shadcn/ui** - Özelleştirilebilir UI bileşenleri
- 📡 **Axios** - HTTP istekleri için
- 📝 **React Hook Form** - Form yönetimi
- 🔄 **TanStack Query** - Sunucu durumu yönetimi
- 🗄️ **Zustand** - Basit ve güçlü state management
- 🔔 **React Toastify** - Bildirimler

## 🎨 Tasarım Sistemi

### Renk Paleti
- **Warm Gray** - Sıcak gri arka plan
- **Sage Green** - Sakin yeşil vurgular
- **Beige & Sand** - Doğal nötr tonlar

### Tipografi
- **Font**: Inter (300, 400, 500, 600, 700)
- **Stil**: Hafif font ağırlıkları, generous whitespace

### Animasyonlar
- Yumuşak ve yavaş geçişler (300-600ms)
- Subtle hover efektleri
- Scale transformasyonları

## 🚀 Kurulum

1. **Bağımlılıkları yükleyin:**
```bash
npm install
```

2. **Ortam değişkenlerini ayarlayın:**
`.env.local` dosyası oluşturun:
```env
NEXT_PUBLIC_API_URL=http://localhost:8080/api
```

3. **Geliştirme sunucusunu başlatın:**
```bash
npm run dev
```

4. Tarayıcınızda [http://localhost:3000](http://localhost:3000) adresini açın

## 📁 Proje Yapısı

```
sql-integration-hub-ui/
├── src/
│   ├── app/              # Next.js App Router
│   │   ├── layout.tsx    # Root layout
│   │   ├── page.tsx      # Ana sayfa
│   │   └── globals.css   # Global stiller
│   ├── components/       # React bileşenleri
│   │   ├── ui/          # shadcn/ui bileşenleri
│   │   └── example-form.tsx
│   ├── hooks/           # Custom React hooks
│   │   └── useExample.ts
│   ├── lib/             # Yardımcı fonksiyonlar
│   │   ├── axios.ts     # Axios yapılandırması
│   │   └── utils.ts     # Utility fonksiyonları
│   ├── providers/       # Context providers
│   │   ├── query-provider.tsx
│   │   └── toast-provider.tsx
│   └── store/           # Zustand stores
│       └── useStore.ts
├── components.json      # shadcn/ui config
├── tailwind.config.ts   # Tailwind yapılandırması
└── tsconfig.json        # TypeScript config
```

## 🛠️ Kullanılan Teknolojiler

- **Framework**: Next.js 14.2.33
- **React**: 19.x
- **TypeScript**: 5.x
- **Styling**: TailwindCSS 3.x
- **UI Components**: shadcn/ui
- **HTTP Client**: Axios
- **Form Management**: React Hook Form
- **Data Fetching**: TanStack Query (React Query)
- **State Management**: Zustand
- **Notifications**: React Toastify

## 📝 Kullanım Örnekleri

### API Hook Oluşturma

```typescript
// src/hooks/useData.ts
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';

export function useData() {
  return useQuery({
    queryKey: ['data'],
    queryFn: async () => {
      const response = await apiClient.get('/endpoint');
      return response.data;
    },
  });
}
```

### Form Oluşturma

```typescript
import { useForm } from 'react-hook-form';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

export function MyForm() {
  const { register, handleSubmit } = useForm();
  
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Input {...register('name')} placeholder="Name" />
      <Button type="submit">Submit</Button>
    </form>
  );
}
```

### State Management

```typescript
import { useStore } from '@/store/useStore';

export function Component() {
  const { user, setUser } = useStore();
  
  return <div>{user?.name}</div>;
}
```

## 🎨 Özel Stil Sınıfları

Projede Zen teması için özel utility sınıflar eklenmiştir:

- `.zen-card` - Hover efektli kart
- `.zen-button` - Yumuşak animasyonlu buton
- `.zen-input` - Odaklanma efektli input
- `.zen-container` - Merkezi container

## 🔧 Geliştirme

### Build

```bash
npm run build
```

### Production

```bash
npm start
```

### Lint

```bash
npm run lint
```

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 🤝 Katkıda Bulunma

Katkılarınızı bekliyoruz! Pull request göndermekten çekinmeyin.

---

**Designed with serenity in mind · Zen aesthetic** 🧘‍♂️
