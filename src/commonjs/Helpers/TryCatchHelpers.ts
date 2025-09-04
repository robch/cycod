export class TryCatchHelpers {
    public static NoThrowWrap(action: (() => void) | null): (() => void) | null {
        return action == null ? null : () => {
            try { action(); }
            catch { }
        };
    }

    public static NoThrowWrapT<T>(action: ((x: T) => void) | null): ((x: T) => void) | null {
        return action == null ? null : (x => {
            try { action(x); }
            catch { }
        });
    }

    public static NoThrowWrapTT<T1, T2>(action: ((x: T1, y: T2) => void) | null): ((x: T1, y: T2) => void) | null {
        return action == null ? null : (x, y) => {
            try { action(x, y); }
            catch { }
        };
    }

    public static NoThrowWrapTTT<T1, T2, T3>(action: ((x: T1, y: T2, z: T3) => void) | null): ((x: T1, y: T2, z: T3) => void) | null {
        return action == null ? null : (x, y, z) => {
            try { action(x, y, z); }
            catch { }
        };
    }

    public static NoThrowWrapTTTT<T1, T2, T3, T4>(action: ((x: T1, y: T2, z: T3, a: T4) => void) | null): ((x: T1, y: T2, z: T3, a: T4) => void) | null {
        return action == null ? null : (x, y, z, a) => {
            try { action(x, y, z, a); }
            catch { }
        };
    }

    public static NoThrowWrapException<T>(func: () => T, defaultResult: T): () => { value: T; ex: Error | null } {
        return () => {
            try {
                return { value: func(), ex: null };
            }
            catch (ex) {
                return { value: defaultResult, ex: ex as Error };
            }
        };
    }

    public static NoThrowWrapExceptionT<T1, T2>(func: (x: T1) => T2, defaultResult: T2): (x: T1) => { value: T2; ex: Error | null } {
        return (x) => {
            try {
                return { value: func(x), ex: null };
            }
            catch (ex) {
                return { value: defaultResult, ex: ex as Error };
            }
        };
    }

    public static NoThrowWrapExceptionTT<T1, T2, T3>(func: (x: T1, y: T2) => T3, defaultResult: T3): (x: T1, y: T2) => { value: T3; ex: Error | null } {
        return (x, y) => {
            try {
                return { value: func(x, y), ex: null };
            }
            catch (ex) {
                return { value: defaultResult, ex: ex as Error };
            }
        };
    }

    public static NoThrowWrapExceptionTTT<T1, T2, T3, T4>(func: (x: T1, y: T2, z: T3) => T4, defaultResult: T4): (x: T1, y: T2, z: T3) => { value: T4; ex: Error | null } {
        return (x, y, z) => {
            try {
                return { value: func(x, y, z), ex: null };
            }
            catch (ex) {
                return { value: defaultResult, ex: ex as Error };
            }
        };
    }

    public static NoThrowWrapExceptionTTTT<T1, T2, T3, T4, T5>(func: (x: T1, y: T2, z: T3, a: T4) => T5, defaultResult: T5): (x: T1, y: T2, z: T3, a: T4) => { value: T5; ex: Error | null } {
        return (x, y, z, a) => {
            try {
                return { value: func(x, y, z, a), ex: null };
            }
            catch (ex) {
                return { value: defaultResult, ex: ex as Error };
            }
        };
    }

    public static TryCatchNoThrow<T>(func: () => T, defaultResult: T): { result: T; exception: Error | null } {
        try {
            return { result: func(), exception: null };
        }
        catch (ex) {
            return { result: defaultResult, exception: ex as Error };
        }
    }
}