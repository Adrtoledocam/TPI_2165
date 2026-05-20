import express from "express";
import { verifyToken } from "../middleware/auth.mjs";
import { getWishlist, moveToCollection, deleteFromWishlist } from "../controllers/wishlistController.mjs";

const router = express.Router();

router.get("/", verifyToken, getWishlist);
router.put("/:id/move", verifyToken, moveToCollection);
router.delete("/:id", verifyToken, deleteFromWishlist);

export default router;
