import { Options } from '@mikro-orm/core';

import { User } from "../entities/UserEntity";

/** 
 * Mikro ORM Connection options object
 * If using a different database other than Mongo DB change 
 * the "type" as necessary following the guidelines here: https://mikro-orm.io/docs/usage-with-sql
 *  */
const options: Options = {
  type: 'mongo',
  entities: [User],
  dbName: 'TechDemo4'
};

export default options;