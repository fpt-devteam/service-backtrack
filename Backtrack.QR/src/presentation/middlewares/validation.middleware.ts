import { createError } from "@/src/shared/core/error.js";
import type { NextFunction, Request, Response } from "express";
import { z } from "zod";

type ZodIssueV4 = z.core.$ZodIssue;

const formatZodIssues = (issues: ZodIssueV4[]) =>
  issues.map((i) => ({
    path: i.path.join("."),
    message: i.message,
    code: i.code,
  }));
export const validateBody =
  <T>(schema: z.ZodSchema<T>) =>
    (req: Request, _res: Response, next: NextFunction) => {
      const parsed = schema.safeParse(req.body);
      if (!parsed.success) {
        return next(createError("Validation", "ValidationError", "Invalid request body", formatZodIssues(parsed.error.issues)));
      }
      // sanitize + typed runtime
      req.body = parsed.data as any;
      next();
    };

export const validateParams =
  <T>(schema: z.ZodSchema<T>) =>
    (req: Request, _res: Response, next: NextFunction) => {
      const parsed = schema.safeParse(req.params);
      if (!parsed.success) {
        return next(createError("Validation", "ValidationError", "Invalid route params", formatZodIssues(parsed.error.issues)));
      }
      req.params = parsed.data as any;
      next();
    };

export const validateQuery =
  <T>(schema: z.ZodSchema<T>) =>
    (req: Request, _res: Response, next: NextFunction) => {
      const parsed = schema.safeParse(req.query);
      if (!parsed.success) {
        return next(createError("Validation", "ValidationError", "Invalid query string", formatZodIssues(parsed.error.issues)));
      }
      req.query = parsed.data as any;
      next();
    };
