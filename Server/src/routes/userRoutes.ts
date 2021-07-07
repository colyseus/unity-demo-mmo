import express from "express";
import * as authController from "../controllers/authController";

const router = express.Router();

// Register our sign up and login routes
router.post("/signup", authController.prepEmail, authController.signUp);
router.post("/login", authController.prepEmail, authController.logIn);

export default router;