
export class Vector{

}

export class Vector3 extends Vector {
    x: number;
    y: number;
    z: number;

    constructor(x: number = 0, y: number = 0, z: number = 0) {
        super();

        this.x = x;
        this.y = y;
        this.z = z;
    }
}

export class Vector2 extends Vector {
    x: number;
    y: number;

    constructor(x: number = 0, y: number = 0){
        super();

        this.x = x;
        this.y = y;
    }
}