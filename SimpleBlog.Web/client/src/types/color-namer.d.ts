declare module 'color-namer' {
  export interface ColorName {
    name: string;
    hex: string;
  }

  export interface ColorNamerResult {
    basic?: ColorName[];
    ntc?: ColorName[];
    [key: string]: ColorName[] | undefined;
  }

  export default function colorNamer(color: string): ColorNamerResult;
}
