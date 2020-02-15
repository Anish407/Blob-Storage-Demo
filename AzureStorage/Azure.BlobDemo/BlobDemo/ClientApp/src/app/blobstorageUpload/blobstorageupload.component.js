"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var BlobUploadComponent = /** @class */ (function () {
    function BlobUploadComponent(httpClient) {
        this.httpClient = httpClient;
        this.title = "blob sample";
        this.fileToUpload = null;
    }
    BlobUploadComponent.prototype.uploadToBLob = function (files) {
        this.fileToUpload = files.item(0);
        var formData = new FormData();
        formData.append('fileKey', this.fileToUpload, this.fileToUpload.name);
        this.httpClient
            .post("", formData)
            .subscribe(function (event) {
        });
    };
    BlobUploadComponent.prototype.UploadFile = function (files, FileName) {
        var formData = new FormData();
        files.forEach(function (f) { return formData.append("files", f); });
        this.httpClient
            .post("PathToAPI" + FileName, formData)
            .subscribe(function (event) { });
    };
    BlobUploadComponent = __decorate([
        core_1.Component({
            selector: 'blob-upload-comp',
            templateUrl: './blob.component.html',
            styleUrls: ['/blob.component.css']
        })
    ], BlobUploadComponent);
    return BlobUploadComponent;
}());
exports.BlobUploadComponent = BlobUploadComponent;
//# sourceMappingURL=blobstorageupload.component.js.map