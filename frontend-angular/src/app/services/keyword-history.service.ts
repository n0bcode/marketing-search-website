import { Injectable } from '@angular/core';
import { openDB, DBSchema, IDBPDatabase } from 'idb';
import { KeywordModel } from '../models/keyword-model';

interface MarketingDBSchema extends DBSchema {
  keywords: {
    key: number;
    value: KeywordModel;
    indexes: { createdAt: Date };
  };
}

@Injectable({
  providedIn: 'root',
})
export class KeywordHistoryService {
  private dbPromise: Promise<IDBPDatabase<MarketingDBSchema>>;

  constructor() {
    this.dbPromise = openDB<MarketingDBSchema>('marketingSearchDB', 1, {
      upgrade(db) {
        const store = db.createObjectStore('keywords', {
          keyPath: 'id',
          autoIncrement: true,
        });
        store.createIndex('createdAt', 'createdAt');
      },
    });
  }

  async addKeyword(keyword: string): Promise<void> {
    const db = await this.dbPromise;
    const tx = db.transaction('keywords', 'readwrite');
    const store = tx.objectStore('keywords');
    const newKeyword: KeywordModel = {
      keyword: keyword,
      createdAt: new Date(),
      updatedAt: new Date(),
      id: Date.now().toString(), // Temporary unique ID
      relatedKeyword: '',
      source: '',
      term: '',
      socialMediaInfo: '',
      note: '',
    };
    await store.add(newKeyword);
    await tx.done;
  }

  async getAllKeywords(): Promise<KeywordModel[]> {
    const db = await this.dbPromise;
    return db.getAllFromIndex('keywords', 'createdAt');
  }

  async deleteKeyword(id: number): Promise<void> {
    const db = await this.dbPromise;
    const tx = db.transaction('keywords', 'readwrite');
    const store = tx.objectStore('keywords');
    await store.delete(id);
    await tx.done;
  }
}
