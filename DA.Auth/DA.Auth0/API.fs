﻿[<AutoOpen>]
module DA.Auth0.API

open DA.Auth0
open DA.FSX
open System.Threading.Tasks

open DA.FSX.ReaderTask
open DA.FSX.Reader
open FSharpx.Reader
open DA.FSX.HttpTask
open DA.Auth0.RequestAPI
open DA.Auth.Domain

type HttpRequest = Request -> Task<string>

type Auth0APIConfig = HttpRequest * Auth0Config

type API<'a> = ReaderTask<Auth0APIConfig, 'a>

// Response DTOs

type UserIdResponse = { user_id : string }

type TokensResponse = {
    id_token: string
    access_token: string
    refresh_token: string
}

type ManagementTokenResponse = {
    token_type: string
    access_token: string
}

// Domain 


let mapResponse f x = ReaderTask.map(str2json >> f) x

// get management token

let mapManagementTokenResponse = mapResponse(fun x -> 
        x.token_type + " " + x.access_token
    )

let managementToken' (f: HttpRequest) = (f <!> getManagementToken) |> mapManagementTokenResponse

let managementToken = managementToken' |> flat

let managementTokenMem: API<string> = Task.memoize(managementToken)

//

let mapUserIdResponse = mapResponse(fun x -> x.user_id)

let inline withToken f = managementTokenMem |> ReaderTask.bind f

// create user

let createUser'' userInfo token (f: HttpRequest) = (f <!> createUser userInfo token) |> mapUserIdResponse

let createUser' userInfo token = createUser'' userInfo token |> flat

let createUser = createUser' >> withToken 

// let createUser userInfo = getManagementToken |> ReaderTask.bind (createUserWithToken userInfo)
    
// get user (by user token ?)

let getUser' token (f: HttpRequest) = (f <!> getUser token) |> mapUserIdResponse

let getUser = getUser' >> flat

// user tokens 

let mapTokensResponse x = x |> mapResponse(fun x -> 
    {
        idToken = x.id_token
        accessToken = x.access_token
        refreshToken = x.refresh_token
    })

let getUserTokens'' userInfo (f: HttpRequest) = (f <!> getUserTokens userInfo) |> mapTokensResponse

let getUserTokens = getUserTokens'' >> flat

// register user

let registerUser userInfo =    
    readerTask {
        let! userId = createUser userInfo
        let! tokens = getUserTokens userInfo
        return { userId = userId; tokens = tokens }
    }    

// remove user

let removeUser'' userId token (f: HttpRequest) = (f <!> removeUser token userId) |> mapResponse(fun _ -> true)

let removeUser' userId token = removeUser'' userId token |> flat

let removeUser = removeUser' >> withToken