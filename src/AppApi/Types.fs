namespace AppApi


type User = { Id: string; Name: string }

and Todo =
    { Id: string
      Owner: User
      Title: string
      Description: string option }
