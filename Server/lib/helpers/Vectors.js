"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Vector2 = exports.Vector3 = exports.Vector = void 0;
class Vector {
}
exports.Vector = Vector;
class Vector3 extends Vector {
    constructor(x = 0, y = 0, z = 0) {
        super();
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
exports.Vector3 = Vector3;
class Vector2 extends Vector {
    constructor(x = 0, y = 0) {
        super();
        this.x = x;
        this.y = y;
    }
}
exports.Vector2 = Vector2;
