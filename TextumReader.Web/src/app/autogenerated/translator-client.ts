/* tslint:disable */
/* eslint-disable */
//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.11.1.0 (NJsonSchema v10.4.3.0 (Newtonsoft.Json v11.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------
// ReSharper disable InconsistentNaming

import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';

export const TranslatorBaseUrl = new InjectionToken<string>('TranslatorBaseUrl');

@Injectable({
    providedIn: 'root'
})
export class TranslatorClient {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(TranslatorBaseUrl) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    getWordTranslation(body: TranslationRequest | undefined): Observable<WordTranslationsDto> {
        let url_ = this.baseUrl + "/translator/word-translation";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "text/plain"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetWordTranslation(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetWordTranslation(<any>response_);
                } catch (e) {
                    return <Observable<WordTranslationsDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<WordTranslationsDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetWordTranslation(response: HttpResponseBase): Observable<WordTranslationsDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }}
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = WordTranslationsDto.fromJS(resultData200);
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<WordTranslationsDto>(<any>null);
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    getExamples(body: WordExampleRequest | undefined): Observable<string[]> {
        let url_ = this.baseUrl + "/translator/word-examples";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "text/plain"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetExamples(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetExamples(<any>response_);
                } catch (e) {
                    return <Observable<string[]>><any>_observableThrow(e);
                }
            } else
                return <Observable<string[]>><any>_observableThrow(response_);
        }));
    }

    protected processGetExamples(response: HttpResponseBase): Observable<string[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }}
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            if (Array.isArray(resultData200)) {
                result200 = [] as any;
                for (let item of resultData200)
                    result200!.push(item);
            }
            else {
                result200 = <any>null;
            }
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<string[]>(<any>null);
    }

    /**
     * @param body (optional) 
     * @return Success
     */
    getTextTranslation(body: TranslationRequest | undefined): Observable<TextTranslationDto> {
        let url_ = this.baseUrl + "/translator/text-translation";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(body);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "text/plain"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetTextTranslation(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetTextTranslation(<any>response_);
                } catch (e) {
                    return <Observable<TextTranslationDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<TextTranslationDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetTextTranslation(response: HttpResponseBase): Observable<TextTranslationDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }}
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = TextTranslationDto.fromJS(resultData200);
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<TextTranslationDto>(<any>null);
    }
}

export class TextTranslationDto implements ITextTranslationDto {
    translation?: string | undefined;

    constructor(data?: ITextTranslationDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.translation = _data["translation"];
        }
    }

    static fromJS(data: any): TextTranslationDto {
        data = typeof data === 'object' ? data : {};
        let result = new TextTranslationDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["translation"] = this.translation;
        return data; 
    }
}

export interface ITextTranslationDto {
    translation?: string | undefined;
}

export class TranslationRequest implements ITranslationRequest {
    from?: string | undefined;
    to?: string | undefined;
    text?: string | undefined;

    constructor(data?: ITranslationRequest) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.from = _data["from"];
            this.to = _data["to"];
            this.text = _data["text"];
        }
    }

    static fromJS(data: any): TranslationRequest {
        data = typeof data === 'object' ? data : {};
        let result = new TranslationRequest();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["from"] = this.from;
        data["to"] = this.to;
        data["text"] = this.text;
        return data; 
    }
}

export interface ITranslationRequest {
    from?: string | undefined;
    to?: string | undefined;
    text?: string | undefined;
}

export class WordExampleRequest implements IWordExampleRequest {
    from?: string | undefined;
    to?: string | undefined;
    text?: string | undefined;
    translation?: string | undefined;

    constructor(data?: IWordExampleRequest) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.from = _data["from"];
            this.to = _data["to"];
            this.text = _data["text"];
            this.translation = _data["translation"];
        }
    }

    static fromJS(data: any): WordExampleRequest {
        data = typeof data === 'object' ? data : {};
        let result = new WordExampleRequest();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["from"] = this.from;
        data["to"] = this.to;
        data["text"] = this.text;
        data["translation"] = this.translation;
        return data; 
    }
}

export interface IWordExampleRequest {
    from?: string | undefined;
    to?: string | undefined;
    text?: string | undefined;
    translation?: string | undefined;
}

export class WordTranslationDto implements IWordTranslationDto {
    partOfSpeech?: string | undefined;
    translation?: string | undefined;

    constructor(data?: IWordTranslationDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.partOfSpeech = _data["partOfSpeech"];
            this.translation = _data["translation"];
        }
    }

    static fromJS(data: any): WordTranslationDto {
        data = typeof data === 'object' ? data : {};
        let result = new WordTranslationDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["partOfSpeech"] = this.partOfSpeech;
        data["translation"] = this.translation;
        return data; 
    }
}

export interface IWordTranslationDto {
    partOfSpeech?: string | undefined;
    translation?: string | undefined;
}

export class WordTranslationsDto implements IWordTranslationsDto {
    word?: string | undefined;
    translations?: WordTranslationDto[] | undefined;

    constructor(data?: IWordTranslationsDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.word = _data["word"];
            if (Array.isArray(_data["translations"])) {
                this.translations = [] as any;
                for (let item of _data["translations"])
                    this.translations!.push(WordTranslationDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): WordTranslationsDto {
        data = typeof data === 'object' ? data : {};
        let result = new WordTranslationsDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["word"] = this.word;
        if (Array.isArray(this.translations)) {
            data["translations"] = [];
            for (let item of this.translations)
                data["translations"].push(item.toJSON());
        }
        return data; 
    }
}

export interface IWordTranslationsDto {
    word?: string | undefined;
    translations?: WordTranslationDto[] | undefined;
}

export class ApiException extends Error {
    message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;

    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();

        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }

    protected isApiException = true;

    static isApiException(obj: any): obj is ApiException {
        return obj.isApiException === true;
    }
}

function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
    if (result !== null && result !== undefined)
        return _observableThrow(result);
    else
        return _observableThrow(new ApiException(message, status, response, headers, null));
}

function blobToText(blob: any): Observable<string> {
    return new Observable<string>((observer: any) => {
        if (!blob) {
            observer.next("");
            observer.complete();
        } else {
            let reader = new FileReader();
            reader.onload = event => {
                observer.next((<any>event.target).result);
                observer.complete();
            };
            reader.readAsText(blob);
        }
    });
}