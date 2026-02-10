// Intelligent Request Caching with TTL and Invalidation

interface RequestCache<T = any> {
  key: string;
  data: T;
  timestamp: number;
  ttl: number;
}

export class CacheManager {
  private static cache = new Map<string, RequestCache>();
  private static readonly MAX_CACHE_SIZE = 1000;

  static set(key: string, data: any, ttl: number = 300000): void {
    // Implement LRU eviction if cache is full
    if (this.cache.size >= this.MAX_CACHE_SIZE) {
      const oldestKey = this.cache.keys().next().value;
      if (oldestKey) {
        this.cache.delete(oldestKey);
      }
    }

    this.cache.set(key, {
      key,
      data,
      timestamp: Date.now(),
      ttl
    });
  }

  static get<T>(key: string): T | null {
    const cached = this.cache.get(key);
    
    if (!cached) {
      return null;
    }

    // Check if cache has expired
    if (Date.now() - cached.timestamp > cached.ttl) {
      this.cache.delete(key);
      return null;
    }

    // Move to end (LRU)
    this.cache.delete(key);
    this.cache.set(key, cached);
    
    return cached.data;
  }

  static delete(key: string): void {
    this.cache.delete(key);
  }

  static clear(): void {
    this.cache.clear();
  }

  static generateKey(url: string, method: string, body?: any): string {
    const bodyHash = body ? this.hashString(JSON.stringify(body)) : '';
    return `${method}:${url}:${bodyHash}`;
  }

  static invalidatePattern(pattern: RegExp): void {
    for (const key of this.cache.keys()) {
      if (pattern.test(key)) {
        this.cache.delete(key);
      }
    }
  }

  static getStats(): { size: number; maxSize: number; hitRate: number } {
    return {
      size: this.cache.size,
      maxSize: this.MAX_CACHE_SIZE,
      hitRate: 0 // TODO: Implement hit rate tracking
    };
  }

  private static hashString(str: string): string {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return hash.toString();
  }
}

// Cache strategies
export class CacheStrategies {
  static async noCache<T>(fetchFn: () => Promise<T>): Promise<T> {
    return fetchFn();
  }

  static async cacheFirst<T>(
    key: string,
    fetchFn: () => Promise<T>,
    ttl: number = 300000
  ): Promise<T> {
    const cached = CacheManager.get<T>(key);
    if (cached !== null) {
      return cached;
    }

    const data = await fetchFn();
    CacheManager.set(key, data, ttl);
    return data;
  }

  static async networkFirst<T>(
    key: string,
    fetchFn: () => Promise<T>,
    ttl: number = 300000
  ): Promise<T> {
    try {
      const data = await fetchFn();
      CacheManager.set(key, data, ttl);
      return data;
    } catch (error) {
      const cached = CacheManager.get<T>(key);
      if (cached !== null) {
        return cached;
      }
      throw error;
    }
  }

  static async cacheOnly<T>(key: string): Promise<T> {
    const cached = CacheManager.get<T>(key);
    if (cached === null) {
      throw new Error('Cache miss');
    }
    return cached;
  }
}
