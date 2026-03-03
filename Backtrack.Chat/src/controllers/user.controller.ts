import { Request, Response } from 'express';
import * as userService from '@/services/user.service';
import { ApiResponseBuilder } from '@/utils/api-response';

export const getAllUsers = async (req: Request, res: Response) => {
	const users = await userService.getAllUsers();
	const response = ApiResponseBuilder.success({ users }, req.headers['x-correlation-id'] as string);
	return res.status(200).json(response);
}
export const createUser = async (req: Request, res: Response) => {
	const user = await userService.createUser(req.body);
	const response = ApiResponseBuilder.success({ user }, req.headers['x-correlation-id'] as string);
	return res.status(201).json(response);
}