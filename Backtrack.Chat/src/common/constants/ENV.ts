import jetEnv, { num } from 'jet-env';

/******************************************************************************
                                 Setup
******************************************************************************/

const ENV = jetEnv({
  NodeEnv: String,
  Port: num,
  MongodbConnectionstring: String,
  // Pagination
  DefaultPageLimit: num,
  MaxPageLimit: num,
});


/******************************************************************************
                          Structured Export
******************************************************************************/

export default {
  NodeEnv: ENV.NodeEnv as string,
  Port: ENV.Port,
  MongodbConnectionstring: ENV.MongodbConnectionstring as string,

  Pagination: {
    DefaultLimit: ENV.DefaultPageLimit,
    MaxLimit: ENV.MaxPageLimit,
  },
} as const;
