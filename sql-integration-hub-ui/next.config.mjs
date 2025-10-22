/** @type {import('next').NextConfig} */
const nextConfig = {
  // Docker için standalone output
  output: 'standalone',
  
  // Production build sırasında ESLint ve TypeScript hatalarını ignore et
  eslint: {
    ignoreDuringBuilds: true,
  },
  typescript: {
    ignoreBuildErrors: true,
  },
  
  webpack: (config) => {
    config.watchOptions = {
      poll: 1000,
      aggregateTimeout: 300,
      ignored: /node_modules/,
    };
    return config;
  },
};

export default nextConfig;
