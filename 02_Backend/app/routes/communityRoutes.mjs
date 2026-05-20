import express from "express";
import { verifyToken } from "../middleware/auth.mjs";
import { getCommunity } from "../controllers/communityController.mjs";

const router = express.Router();

router.get("/", verifyToken, getCommunity);

export default router;
