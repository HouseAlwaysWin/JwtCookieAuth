export interface AuthRes {
  isAuth: boolean,
  userInfo: UserInfo
}

export interface UserInfo {
  id: string,
  name: string,
  email: string,
  pictureUrl: string,
  provider: string
}
