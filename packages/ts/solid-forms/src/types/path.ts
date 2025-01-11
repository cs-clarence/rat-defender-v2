import type { FieldValue, FieldValues } from "./field";
import type { IsTuple, TupleKeys, ArrayKey } from "./utils";

/**
 * Returns a path of a type that leads to a field value.
 */
type ValuePath<TKey extends string | number, TValue> = TValue extends string[]
    ? `${TKey}` | `${TKey}.${ValuePaths<TValue>}`
    : TValue extends FieldValue | Blob
      ? `${TKey}`
      : `${TKey}.${ValuePaths<TValue>}`;

/**
 * Returns all paths of a type that lead to a field value.
 */
type ValuePaths<TValue> = TValue extends Array<infer TChild>
    ? IsTuple<TValue> extends true
        ? {
              [TKey in TupleKeys<TValue>]-?: ValuePath<
                  TKey & string,
                  TValue[TKey]
              >;
          }[TupleKeys<TValue>]
        : ValuePath<ArrayKey, TChild>
    : {
          [TKey in keyof TValue]-?: ValuePath<TKey & string, TValue[TKey]>;
      }[keyof TValue];

type RemoveUndefined<T> = T extends undefined ? never : T;

type RemoveNull<T> = T extends null ? never : T;

type RemoveNullish<T> = RemoveUndefined<RemoveNull<T>>;

type CheckNull<T> = [T] extends [null] ? true : null extends T ? true : false;
type CheckUndefined<T> = [T] extends [undefined]
    ? true
    : undefined extends T
      ? true
      : false;

type TransponseNull<T> = CheckNull<T> extends true
    ? RemoveNull<{
          [TKey in keyof T]-?: T[TKey] | null;
      }>
    : T;

type TransponseUndefined<T> = CheckUndefined<T> extends true
    ? RemoveUndefined<{
          [TKey in keyof T]-?: T[TKey] | undefined;
      }>
    : T;

type TransponseNullish<T> = CheckNull<T> extends true
    ? CheckUndefined<T> extends true
        ? RemoveNullish<{
              [TKey in keyof T]-?: T[TKey] | null | undefined;
          }>
        : TransponseNull<T>
    : CheckUndefined<T> extends true
      ? TransponseUndefined<T>
      : T;

/**
 * See {@link ValuePaths}
 */
export type FieldPath<TFieldValues extends FieldValues> =
    ValuePaths<TFieldValues>;

/**
 * Returns the value of a field value or array path of a type.
 */
type PathValue<
    TValue,
    TPath,
    TTransposeNullish extends boolean = true,
> = TPath extends `${infer TKey1}.${infer TKey2}`
    ? TKey1 extends keyof TValue
        ? TKey2 extends ValuePaths<TValue[TKey1]> | ArrayPaths<TValue[TKey1]>
            ? PathValue<
                  TTransposeNullish extends true
                      ? TransponseNullish<TValue[TKey1]>
                      : TValue[TKey1],
                  TKey2
              >
            : never
        : TKey1 extends `${ArrayKey}`
          ? TValue extends Array<infer TChild>
              ? PathValue<
                    TTransposeNullish extends true
                        ? TransponseNullish<TChild>
                        : TChild,
                    TKey2 & (ValuePaths<TChild> | ArrayPaths<TChild>)
                >
              : never
          : never
    : TPath extends keyof TValue
      ? TValue[TPath]
      : never;

/**
 * See {@link PathValue}
 */
export type FieldPathValue<
    TFieldValues extends FieldValues,
    TFieldPath extends FieldPath<TFieldValues>,
    TTransposeNullish extends boolean = true,
> = PathValue<TFieldValues, TFieldPath, TTransposeNullish>;

/**
 * Returns a path of a type that leads to a field array.
 */
type ArrayPath<
    TKey extends string | number,
    Value,
> = Value extends Array<unknown>
    ? `${TKey}` | `${TKey}.${ArrayPaths<Value>}`
    : Value extends FieldValues
      ? `${TKey}.${ArrayPaths<Value>}`
      : never;

/**
 * Returns all paths of a type that lead to a field array.
 */
type ArrayPaths<TValue> = TValue extends Array<infer TChild>
    ? IsTuple<TValue> extends true
        ? {
              [TKey in TupleKeys<TValue>]-?: ArrayPath<
                  TKey & string,
                  TValue[TKey]
              >;
          }[TupleKeys<TValue>]
        : ArrayPath<ArrayKey, TChild>
    : {
          [TKey in keyof TValue]-?: ArrayPath<TKey & string, TValue[TKey]>;
      }[keyof TValue];

/**
 * See {@link ArrayPaths}
 */
export type FieldArrayPath<TFieldValues extends FieldValues> =
    ArrayPaths<TFieldValues>;

/**
 * See {@link PathValue}
 */
export type FieldArrayPathValue<
    TFieldValues extends FieldValues,
    TFieldArrayPath extends FieldArrayPath<TFieldValues>,
    TTransposeNullish extends boolean = true,
> = PathValue<TFieldValues, TFieldArrayPath, TTransposeNullish> &
    Array<unknown>;
